namespace AgendeAqui.Application.Reports.GetPlatformDashboard;

public sealed record PlatformDashboardResponse(
    int ActiveTenants,
    int TotalTenants,
    int TotalAppointmentsInPeriod,
    IReadOnlyList<TenantAppointmentSummary> Tenants);

public sealed record TenantAppointmentSummary(
    Guid TenantId,
    string TenantName,
    string TenantSlug,
    int Total,
    int Completed,
    int Cancelled,
    int NoShow,
    double CompletionRate,
    int ActiveProfessionals);
