namespace AgendeAqui.Application.Appointments.GetMyCalendar;

public sealed record CalendarResponse(
    IReadOnlyList<CalendarDayResponse> Days);

public sealed record CalendarDayResponse(
    DateOnly Date,
    IReadOnlyList<CalendarAppointmentResponse> Appointments,
    IReadOnlyList<CalendarAbsenceResponse> Absences);

public sealed record CalendarAppointmentResponse(
    Guid Id,
    string ClientName,
    string ServiceName,
    string StartTime,
    string EndTime,
    string Status,
    string? Notes);

public sealed record CalendarAbsenceResponse(
    Guid Id,
    string? StartTime,
    string? EndTime,
    bool IsFullDay,
    string? Reason);
