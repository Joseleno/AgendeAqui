using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Schedules.GetSchedule;

public sealed record GetScheduleQuery(Guid ScheduleId) : IQuery<ScheduleResponse>;
