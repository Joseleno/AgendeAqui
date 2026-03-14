using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Schedules.DetectConflicts;

public sealed record DetectScheduleConflictsQuery(DateOnly Date) : IQuery<List<ScheduleConflictResponse>>;
