using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Appointments.Events;

public sealed record AppointmentRescheduledEvent(Guid AppointmentId, DateOnly NewDate, Guid? SourceApiKeyId = null) : IDomainEvent;
