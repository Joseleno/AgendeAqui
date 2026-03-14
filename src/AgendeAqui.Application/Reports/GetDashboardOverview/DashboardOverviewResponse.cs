namespace AgendeAqui.Application.Reports.GetDashboardOverview;

public sealed record DashboardOverviewResponse(
    int AppointmentsToday,
    int AppointmentsThisWeek,
    int ActiveProfessionals,
    int RegisteredClients,
    int CancelledToday,
    int NoShowToday);
