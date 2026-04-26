using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Tenants.GetTenantContext;

public sealed record GetTenantContextQuery : IQuery<TenantContextResponse>;
