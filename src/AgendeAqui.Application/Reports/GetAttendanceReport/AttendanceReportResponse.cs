namespace AgendeAqui.Application.Reports.GetAttendanceReport;

public sealed record AttendanceReportResponse(
    int TotalAppointments,
    int TotalCompleted,
    int TotalCancelled,
    int TotalNoShow,
    List<ProfessionalAttendance> Breakdown);

public sealed record ProfessionalAttendance(
    Guid ProfessionalId,
    string ProfessionalName,
    int Total,
    int Completed,
    int Cancelled,
    int NoShow);
