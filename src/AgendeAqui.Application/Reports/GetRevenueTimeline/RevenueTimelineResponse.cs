namespace AgendeAqui.Application.Reports.GetRevenueTimeline;

public sealed record RevenueTimelineResponse(List<RevenuePoint> Points);

public sealed record RevenuePoint(string Month, decimal Revenue, int AppointmentCount);
