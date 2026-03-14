namespace AgendeAqui.Application.Appointments.GetClinicCalendar;

public sealed record ClinicCalendarResponse(
    DateOnly Date,
    IReadOnlyList<ProfessionalDayResponse> Professionals);

public sealed record ProfessionalDayResponse(
    Guid ProfessionalId,
    string ProfessionalName,
    IReadOnlyList<ClinicAppointmentResponse> Appointments);

public sealed record ClinicAppointmentResponse(
    Guid Id,
    string ClientName,
    string ServiceName,
    string StartTime,
    string EndTime,
    string Status);
