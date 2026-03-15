using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Reports.GetAppointmentsTimeline;

public sealed record GetAppointmentsTimelineQuery(
    DateOnly From,
    DateOnly To,
    string GroupBy = "day") : IQuery<AppointmentsTimelineResponse>;
