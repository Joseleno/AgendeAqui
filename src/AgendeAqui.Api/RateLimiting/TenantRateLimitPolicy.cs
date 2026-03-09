using AgendeAqui.Domain.Tenants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Threading.RateLimiting;

namespace AgendeAqui.Api.RateLimiting;

/// <summary>
/// Maps TenantPlan enum values to fixed-window rate limit permit counts (requests per minute).
/// Free=30, Starter=100, Professional=1000, Enterprise=5000.
/// </summary>
public sealed class TenantRateLimitPolicy : IRateLimiterPolicy<string>
{
    public const int FreePlanLimit = 30;
    public const int StarterPlanLimit = 100;
    public const int ProfessionalPlanLimit = 1000;
    public const int EnterprisePlanLimit = 5000;
    public const int DefaultWindowMinutes = 1;

    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected => WriteRateLimitResponse;

    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        var tenantId = httpContext.User.FindFirstValue("tenant_id") ?? "anonymous";
        var planClaim = httpContext.User.FindFirstValue("tenant_plan");
        var permitLimit = GetPermitLimit(planClaim);

        return RateLimitPartition.GetFixedWindowLimiter(tenantId, _ =>
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromMinutes(DefaultWindowMinutes),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    }

    /// <summary>
    /// Maps the tenant_plan JWT claim (integer string) to request-per-minute limits.
    /// Claim values correspond to TenantPlan enum: 1=Free, 2=Starter, 3=Professional, 4=Enterprise.
    /// </summary>
    public static int GetPermitLimit(string? planClaim)
    {
        if (!int.TryParse(planClaim, out var plan))
            return FreePlanLimit;

        return (TenantPlan)plan switch
        {
            TenantPlan.Starter => StarterPlanLimit,
            TenantPlan.Professional => ProfessionalPlanLimit,
            TenantPlan.Enterprise => EnterprisePlanLimit,
            _ => FreePlanLimit  // Free (1) or unknown
        };
    }

    /// <summary>
    /// Shared RFC 7807 ProblemDetails response for rate limit rejections.
    /// </summary>
    public static async ValueTask WriteRateLimitResponse(OnRejectedContext context, CancellationToken cancellationToken)
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/problem+json";
        await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc6585#section-4",
            Title = "Too Many Requests",
            Status = 429,
            Detail = "Rate limit exceeded. Please try again later."
        }, cancellationToken);
    }
}
