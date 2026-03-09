using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Appointments.Events;

public sealed record AppointmentConfirmedEvent(Guid AppointmentId) : IDomainEvent;
