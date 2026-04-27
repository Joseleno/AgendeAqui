using AgendeAqui.Domain.Tenants;

namespace AgendeAqui.Application.Tenants.GetTenantContext;

public sealed record TenantContextResponse(
    string TenantName,
    string Plan,
    bool IsOnTrial,
    DateTime? TrialEndsAt,
    TenantLabels Labels,
    TenantFeatures Features,
    TenantTheme Theme);
