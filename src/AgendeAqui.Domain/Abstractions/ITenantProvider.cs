namespace AgendeAqui.Domain.Abstractions;

public interface ITenantProvider
{
    Guid GetTenantId();
    void SetTenantId(Guid tenantId);
}
