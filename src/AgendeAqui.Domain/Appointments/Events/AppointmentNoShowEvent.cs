using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Appointments.Events;

public sealed record AppointmentNoShowEvent(Guid AppointmentId) : IDomainEvent;
