using AgendeAqui.Domain.Common;

namespace AgendeAqui.Application.Tenants;

public static class TenantErrors
{
    public static readonly Error SlugAlreadyExists = new("Tenant.SlugAlreadyExists", "A tenant with this slug already exists.");
    public static readonly Error NotFound = new("Tenant.NotFound", "Tenant not found.");
    public static readonly Error InvalidPlan = new("Tenant.InvalidPlan", "Invalid tenant plan.");
    public static readonly Error AlreadySuspended = new("Tenant.AlreadySuspended", "Tenant is already suspended.");
    public static readonly Error CustomDomainAlreadyTaken = new("Tenant.CustomDomainAlreadyTaken", "This custom domain is already registered to another tenant.");
    public static readonly Error CannotExportData = new("Tenant.CannotExportData", "Data export is not available on the current plan.");
}
