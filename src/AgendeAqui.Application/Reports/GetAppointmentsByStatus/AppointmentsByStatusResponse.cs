namespace AgendeAqui.Application.Reports.GetAppointmentsByStatus;

public sealed record AppointmentsByStatusResponse(List<StatusCount> Items);

public sealed record StatusCount(string Status, int Count);
