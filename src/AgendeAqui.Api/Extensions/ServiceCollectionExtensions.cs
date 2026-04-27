using AgendeAqui.Api.Auth;
using AgendeAqui.Api.Hubs;
using AgendeAqui.Api.RateLimiting;
using AgendeAqui.Application;
using AgendeAqui.Application.Abstractions.RealTime;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Infrastructure;
using AgendeAqui.Infrastructure.Messaging;
using AgendeAqui.Infrastructure.Observability;
using RabbitMQ.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.HttpOverrides;
using System.Text;
using System.Threading.RateLimiting;

namespace AgendeAqui.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgendeAqui(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOpenApi()
            .AddApplication()
            .AddInfrastructure(configuration)
            .AddMediator()
            .AddObservability(configuration);

        services.AddMemoryCache();
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromMinutes(2)
            };
        });

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy => policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            var allowedOrigins = configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? [];

            options.AddPolicy("Production", policy => policy
                .WithOrigins(allowedOrigins)
                .WithMethods("GET", "POST", "PUT", "DELETE")
                .WithHeaders(
                    "Authorization",
                    "Content-Type",
                    "X-Tenant-Id",
                    "X-Correlation-Id",
                    "X-Api-Key"));
        });

        var rabbitMqSettings = configuration
            .GetSection("RabbitMq")
            .Get<RabbitMqSettings>() ?? new RabbitMqSettings();

        var vhost = Uri.EscapeDataString(rabbitMqSettings.VirtualHost.TrimStart('/'));
        var rabbitMqUri = new Uri(
            $"amqp://{rabbitMqSettings.UserName}:{rabbitMqSettings.Password}" +
            $"@{rabbitMqSettings.HostName}:{rabbitMqSettings.Port}/{vhost}");

        services.AddSingleton(new ConnectionFactory { Uri = rabbitMqUri });

        services.AddHealthChecks()
            .AddNpgSql(
                configuration.GetConnectionString("Database")!,
                tags: ["ready"])
            .AddRabbitMQ(tags: ["ready"]);

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUserService>();
        services.AddScoped<AgendeAqui.Application.Auth.IJwtTokenGenerator, AgendeAqui.Api.Auth.JwtTokenGenerator>();

        services.AddSignalR();
        services.AddScoped<IAppointmentHubNotifier, AppointmentHubNotifier>();

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        services.AddAgendeAquiAuthentication(configuration);
        services.AddAuthorization(options => options.AddAgendeAquiPolicies());
        services.AddAgendeAquiRateLimiting();

        return services;
    }

    private static IServiceCollection AddAgendeAquiAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT settings are not configured.");

        if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
            throw new InvalidOperationException(
                "JWT SecretKey is not configured. Use 'dotnet user-secrets set \"Jwt:SecretKey\" \"<your-key>\"' to configure it.");

        if (Encoding.UTF8.GetByteCount(jwtSettings.SecretKey) < 32)
            throw new InvalidOperationException(
                "JWT SecretKey must be at least 32 bytes (256 bits) for HS256. Current key is too short.");

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ClockSkew = TimeSpan.Zero
            };

            // SignalR sends JWT via query string for WebSocket connections
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        })
        .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
            "ApiKey", null);

        return services;
    }

    private static IServiceCollection AddAgendeAquiRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = TenantRateLimitPolicy.WriteRateLimitResponse;

            // Named policy used by endpoint groups via .RequireRateLimiting("tenant")
            options.AddPolicy<string, TenantRateLimitPolicy>("tenant");

            // Global limiter applies tenant-plan FixedWindow rate limits to all routes.
            // Health check and metrics endpoints bypass limiting via GetNoLimiter.
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                context =>
                {
                    // Skip rate limiting for health/metrics endpoints
                    var path = context.Request.Path.Value ?? string.Empty;
                    if (path.StartsWith("/health", StringComparison.OrdinalIgnoreCase) ||
                        path.Equals("/metrics", StringComparison.OrdinalIgnoreCase))
                    {
                        return RateLimitPartition.GetNoLimiter<string>("health");
                    }

                    var tenantId = context.User.FindFirstValue("tenant_id");
                    var planClaim = context.User.FindFirstValue("tenant_plan");
                    var permitLimit = TenantRateLimitPolicy.GetPermitLimit(planClaim);

                    var partitionKey = tenantId
                        ?? context.Connection.RemoteIpAddress?.ToString()
                        ?? "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = permitLimit,
                            Window = TimeSpan.FromMinutes(TenantRateLimitPolicy.DefaultWindowMinutes),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        });
                });
        });

        return services;
    }
}
