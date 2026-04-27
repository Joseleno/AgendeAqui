using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Tenants.GetTenantUsage;

public sealed record GetTenantUsageQuery(Guid TenantId) : IQuery<TenantUsageResponse>;
