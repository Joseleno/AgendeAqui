using AgendeAqui.Api.Auth;
using AgendeAqui.Application;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
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
            .AddMediator();

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

            // tenant_plan claim values: 1=Free, 2=Starter, 3=Professional, 4=Enterprise
            options.AddPolicy("tenant", httpContext =>
            {
                var tenantId = httpContext.User.FindFirstValue("tenant_id") ?? "anonymous";
                var planClaim = httpContext.User.FindFirstValue("tenant_plan");
                var permitLimit = GetPermitLimitForPlan(planClaim);

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

    /// <summary>
    /// Maps tenant_plan claim (integer) to request-per-minute limits.
    /// Expected claim values: 1=Free(30), 2=Starter(100), 3=Professional(1000), 4=Enterprise(5000).
    /// </summary>
    private static int GetPermitLimitForPlan(string? planClaim)
    {
        if (!int.TryParse(planClaim, out var plan))
            return 30;

        return plan switch
        {
            2 => 100,   // Starter
            3 => 1000,  // Professional
            4 => 5000,  // Enterprise
            _ => 30     // Free (1) or unknown
        };
    }
}
