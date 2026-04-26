using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Tenants;

namespace AgendeAqui.Application.Tenants.GetTenantContext;

internal sealed class GetTenantContextQueryHandler(
    ITenantRepository tenantRepository,
    ITenantProvider tenantProvider) : IQueryHandler<GetTenantContextQuery, TenantContextResponse>
{
    public async ValueTask<Result<TenantContextResponse>> Handle(GetTenantContextQuery query, CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.GetTenantId();
        var tenant = await tenantRepository.GetByIdAsync(tenantId, cancellationToken);

        if (tenant is null)
            return Result.Failure<TenantContextResponse>(new Error("Tenant.NotFound", "Tenant not found."));

        return Result.Success(new TenantContextResponse(
            TenantName: tenant.Name,
            Labels: tenant.Labels,
            Features: tenant.Features));
    }
}
