using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Reports.GetAppointmentsByStatus;

public sealed record GetAppointmentsByStatusQuery(DateOnly From, DateOnly To) : IQuery<AppointmentsByStatusResponse>;
