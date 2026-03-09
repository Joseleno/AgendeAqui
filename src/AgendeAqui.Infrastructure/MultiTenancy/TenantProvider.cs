using AgendeAqui.Domain.Abstractions;

namespace AgendeAqui.Infrastructure.MultiTenancy;

internal sealed class TenantProvider : ITenantProvider
{
    private Guid _tenantId;

    public Guid GetTenantId() => _tenantId;

    public void SetTenantId(Guid tenantId) => _tenantId = tenantId;
}
