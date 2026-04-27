using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Tenants.RegisterTenant;

public sealed record RegisterTenantCommand(
    string Name,
    string Slug,
    string AdminEmail,
    string AdminPassword,
    string AdminName) : ICommand<RegisterTenantResult>;

public sealed record RegisterTenantResult(Guid TenantId, Guid AdminUserId, DateTime TrialEndsAt);
