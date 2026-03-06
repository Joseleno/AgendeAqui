namespace AgendeAqui.Domain.Common;

public abstract class TenantEntity : Entity
{
    public Guid TenantId { get; protected set; }

    protected TenantEntity() { }

    protected TenantEntity(Guid tenantId)
    {
        TenantId = tenantId;
    }
}
