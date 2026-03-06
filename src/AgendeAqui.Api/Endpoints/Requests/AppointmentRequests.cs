namespace AgendeAqui.Api.Endpoints.Requests;

public sealed record CreateAppointmentRequest(
    Guid ProfessionalId,
    Guid ServiceId,
    Guid ClientId,
    DateOnly Date,
    TimeOnly StartTime,
    string? Notes);

public sealed record CancelAppointmentRequest(string Reason);

public sealed record RescheduleAppointmentRequest(DateOnly NewDate, TimeOnly NewStartTime);

public sealed record UpdateAttendanceRequest(string Action);
