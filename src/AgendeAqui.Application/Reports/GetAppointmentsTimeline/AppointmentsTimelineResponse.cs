namespace AgendeAqui.Application.Reports.GetAppointmentsTimeline;

public sealed record AppointmentsTimelineResponse(List<TimelinePoint> Points);

public sealed record TimelinePoint(
    string Period,
    int Scheduled,
    int Completed,
    int Cancelled,
    int NoShow);
