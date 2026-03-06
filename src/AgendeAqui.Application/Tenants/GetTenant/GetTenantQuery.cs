using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Tenants.GetTenant;

public sealed record GetTenantQuery(Guid TenantId) : IQuery<TenantResponse>;
