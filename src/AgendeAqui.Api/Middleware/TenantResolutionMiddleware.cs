using AgendeAqui.Domain.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgendeAqui.Api.Middleware;

public sealed class TenantResolutionMiddleware(RequestDelegate next)
{
    private const string TenantIdHeader = "X-Tenant-Id";

    private static readonly HashSet<string> ExcludedPaths =
    [
        "/health",
        "/openapi"
    ];

    public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider)
    {
        if (ExcludedPaths.Any(p => context.Request.Path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase)))
        {
            await next(context);
            return;
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

        // Fallback: X-Tenant-Id header (for unauthenticated routes only)
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
