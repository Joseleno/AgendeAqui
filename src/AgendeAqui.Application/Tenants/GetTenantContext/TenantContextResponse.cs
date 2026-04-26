using AgendeAqui.Domain.Tenants;

namespace AgendeAqui.Application.Tenants.GetTenantContext;

public sealed record TenantContextResponse(
    string TenantName,
    TenantLabels Labels,
    TenantFeatures Features);
