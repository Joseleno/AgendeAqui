namespace AgendeAqui.Application.Reports.GetTeamAppointmentsReport;

public sealed record TeamAppointmentsReportResponse(
    int TotalAppointments,
    int TotalCompleted,
    int TotalCancelled,
    int TotalNoShow,
    IReadOnlyList<TeamAppointmentBreakdown> Teams,
    IReadOnlyList<UnassignedProfessionalBreakdown> Unassigned);

public sealed record TeamAppointmentBreakdown(
    Guid TeamId,
    string TeamName,
    int Total,
    int Completed,
    int Cancelled,
    int NoShow,
    double CompletionRate,
    double CancellationRate);

public sealed record UnassignedProfessionalBreakdown(
    Guid ProfessionalId,
    string ProfessionalName,
    int Total,
    int Completed,
    int Cancelled,
    int NoShow);
