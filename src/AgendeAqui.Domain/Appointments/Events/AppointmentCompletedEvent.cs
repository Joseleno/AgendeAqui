using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Appointments.Events;

public sealed record AppointmentCompletedEvent(Guid AppointmentId) : IDomainEvent;
