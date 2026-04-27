using AgendeAqui.Domain.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace AgendeAqui.Api.Middleware;

public sealed class TenantResolutionMiddleware(RequestDelegate next, IMemoryCache cache)
{
    private const string TenantIdHeader = "X-Tenant-Id";
    private static readonly TimeSpan DomainCacheTtl = TimeSpan.FromMinutes(5);

    private static readonly HashSet<string> ExcludedPaths =
    [
        "/health",
        "/openapi",
        "/metrics",
        "/api/v1/auth/login",
        "/api/v1/auth/tenant",
        "/api/v1/onboarding"
    ];

    public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider, ITenantRepository tenantRepository)
    {
        foreach (var excluded in ExcludedPaths)
        {
            if (context.Request.Path.StartsWithSegments(excluded, StringComparison.OrdinalIgnoreCase))
            {
                await next(context);
                return;
            }
        }

        // Primary source: tenant_id claim from authenticated user (JWT or ApiKey)
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = context.User.FindFirstValue("tenant_id");
            if (Guid.TryParse(tenantIdClaim, out var tenantIdFromClaim))
            {
                tenantProvider.SetTenantId(tenantIdFromClaim);
                await next(context);
                return;
            }

            // Authenticated user without tenant_id claim = misconfigured token
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                Title = "Forbidden",
                Status = 403,
                Detail = "Authenticated user does not have a valid tenant_id claim."
            });
            return;
        }

        // Second fallback: host-based resolution (custom domain)
        // Cached (5 min TTL) to avoid a DB hit on every unauthenticated request.
        // Domain changes take up to 5 minutes to propagate — acceptable for operator-driven ops.
        var host = context.Request.Host.Host;
        if (!string.IsNullOrWhiteSpace(host))
        {
            var cacheKey = $"domain:{host}";
            if (!cache.TryGetValue(cacheKey, out Guid? cachedTenantId))
            {
                var tenantByDomain = await tenantRepository.GetByCustomDomainAsync(host, context.RequestAborted);
                cachedTenantId = tenantByDomain?.Id;
                cache.Set(cacheKey, cachedTenantId, DomainCacheTtl);
            }

            if (cachedTenantId is not null)
            {
                tenantProvider.SetTenantId(cachedTenantId.Value);
                await next(context);
                return;
            }
        }

        // Third fallback: X-Tenant-Id header (for unauthenticated routes only)
        if (!context.Request.Headers.TryGetValue(TenantIdHeader, out var tenantIdHeader)
            || !Guid.TryParse(tenantIdHeader, out var tenantId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Bad Request",
                Status = 400,
                Detail = "X-Tenant-Id header is required and must be a valid GUID."
            });
            return;
        }

        tenantProvider.SetTenantId(tenantId);
        await next(context);
    }
}
