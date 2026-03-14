namespace AgendeAqui.Application.Appointments.ListMyAppointments;

public sealed record MyAppointmentResponse(
    Guid Id,
    string ProfessionalName,
    string ServiceName,
    DateOnly Date,
    string StartTime,
    string EndTime,
    string Status,
    string? Notes);
