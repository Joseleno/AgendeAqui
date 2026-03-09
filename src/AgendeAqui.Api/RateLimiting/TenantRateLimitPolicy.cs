using AgendeAqui.Domain.Tenants;
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
    public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected => null;

    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        var tenantId = httpContext.User.FindFirstValue("tenant_id") ?? "anonymous";
        var planClaim = httpContext.User.FindFirstValue("tenant_plan");
        var permitLimit = GetPermitLimit(planClaim);

        return RateLimitPartition.GetFixedWindowLimiter(tenantId, _ =>
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromMinutes(1),
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
            return 30;

        return (TenantPlan)plan switch
        {
            TenantPlan.Starter => 100,
            TenantPlan.Professional => 1000,
            TenantPlan.Enterprise => 5000,
            _ => 30  // Free (1) or unknown
        };
    }
}
