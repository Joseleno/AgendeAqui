using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Schedules.CreateSchedule;

public sealed record CreateScheduleCommand(
    Guid ProfessionalId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int SlotDurationMinutes) : ICommand<Guid>;
