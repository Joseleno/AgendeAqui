namespace AgendeAqui.Application.Appointments.GetAppointment;

public sealed record AppointmentResponse(
    Guid Id,
    Guid ProfessionalId,
    string ProfessionalName,
    Guid ServiceId,
    string ServiceName,
    Guid ClientId,
    string ClientName,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Status,
    string? Notes,
    DateTime CreatedAt);
