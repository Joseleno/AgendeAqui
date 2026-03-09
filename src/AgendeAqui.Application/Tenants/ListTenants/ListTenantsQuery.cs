using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Common;
using AgendeAqui.Application.Tenants.GetTenant;

namespace AgendeAqui.Application.Tenants.ListTenants;

public sealed record ListTenantsQuery : PagedRequest, IQuery<PagedResponse<TenantResponse>>
{
    public string? Status { get; init; }
}
