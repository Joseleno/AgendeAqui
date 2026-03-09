namespace AgendeAqui.Api.Endpoints.Requests;

public sealed record CreateScheduleRequest(
    Guid ProfessionalId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int SlotDurationMinutes);

public sealed record UpdateScheduleRequest(
    TimeOnly StartTime,
    TimeOnly EndTime,
    int SlotDurationMinutes);
