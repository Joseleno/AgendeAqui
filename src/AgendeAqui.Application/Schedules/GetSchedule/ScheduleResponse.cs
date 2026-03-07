namespace AgendeAqui.Application.Schedules.GetSchedule;

public sealed record ScheduleResponse(
    Guid Id,
    Guid ProfessionalId,
    string ProfessionalName,
    int DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int SlotDurationMinutes,
    bool IsActive,
    DateTime CreatedAt);
