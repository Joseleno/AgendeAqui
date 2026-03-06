using AgendeAqui.Application.Tenants.GetTenant;
using AgendeAqui.Domain.Tenants;
using Riok.Mapperly.Abstractions;

namespace AgendeAqui.Infrastructure.Mapping;

[Mapper]
internal sealed partial class TenantMapper
{
    [MapperIgnoreSource(nameof(Tenant.ConnectionString))]
    [MapperIgnoreSource(nameof(Tenant.UpdatedAt))]
    public partial TenantResponse ToResponse(Tenant tenant);

    private static string MapStatus(TenantStatus status) => status.ToString();
    private static string MapPlan(TenantPlan plan) => plan.ToString();
}
