using AgendeAqui.Domain.Abstractions;

namespace AgendeAqui.Infrastructure.MultiTenancy;

internal sealed class TenantProvider : ITenantProvider
{
    private static readonly AsyncLocal<Guid> _tenantId = new();

    public Guid GetTenantId() => _tenantId.Value;

    public void SetTenantId(Guid tenantId) => _tenantId.Value = tenantId;
}
