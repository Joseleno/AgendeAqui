using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Schedules.UpdateSchedule;

public sealed record UpdateScheduleCommand(
    Guid ScheduleId,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int SlotDurationMinutes) : ICommand;
