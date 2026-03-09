using AgendeAqui.Api.Auth;
using AgendeAqui.Api.RateLimiting;
using AgendeAqui.Application;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Infrastructure;
using AgendeAqui.Infrastructure.Observability;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
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

        services.AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("Database")!);

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUserService>();

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

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.ContentType = "application/problem+json";
                await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc6585#section-4",
                    Title = "Too Many Requests",
                    Status = 429,
                    Detail = "Rate limit exceeded. Please try again later."
                }, cancellationToken);
            };

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

                    var tenantId = context.User.FindFirstValue("tenant_id") ?? "anonymous";
                    var planClaim = context.User.FindFirstValue("tenant_plan");
                    var permitLimit = TenantRateLimitPolicy.GetPermitLimit(planClaim);

                    return RateLimitPartition.GetFixedWindowLimiter(tenantId, _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = permitLimit,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        });
                });
        });

        return services;
    }
}
