namespace AgendeAqui.Application.Tenants.GetTenant;

public sealed record TenantResponse(
    Guid Id,
    string Name,
    string Slug,
    string Status,
    string Plan,
    DateTime CreatedAt);
