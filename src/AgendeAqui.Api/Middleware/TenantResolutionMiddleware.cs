using AgendeAqui.Domain.Abstractions;

namespace AgendeAqui.Api.Middleware;

public sealed class TenantResolutionMiddleware(RequestDelegate next)
{
    private const string TenantIdHeader = "X-Tenant-Id";

    public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider)
    {
        if (context.Request.Headers.TryGetValue(TenantIdHeader, out var tenantIdHeader)
            && Guid.TryParse(tenantIdHeader, out var tenantId))
        {
            tenantProvider.SetTenantId(tenantId);
        }

        await next(context);
    }
}
