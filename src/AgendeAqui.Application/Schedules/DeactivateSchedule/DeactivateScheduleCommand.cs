using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Schedules.DeactivateSchedule;

public sealed record DeactivateScheduleCommand(Guid ScheduleId) : ICommand;
