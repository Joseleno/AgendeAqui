using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Appointments.GetAppointment;

public sealed record GetAppointmentQuery(Guid AppointmentId) : IQuery<AppointmentResponse>;
