using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Reports.GetRevenueTimeline;

public sealed record GetRevenueTimelineQuery(DateOnly From, DateOnly To) : IQuery<RevenueTimelineResponse>;
