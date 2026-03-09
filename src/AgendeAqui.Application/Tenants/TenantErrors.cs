using AgendeAqui.Domain.Common;

namespace AgendeAqui.Application.Tenants;

public static class TenantErrors
{
    public static readonly Error SlugAlreadyExists = new("Tenant.SlugAlreadyExists", "A tenant with this slug already exists.");
    public static readonly Error NotFound = new("Tenant.NotFound", "Tenant not found.");
    public static readonly Error InvalidPlan = new("Tenant.InvalidPlan", "Invalid tenant plan.");
}
