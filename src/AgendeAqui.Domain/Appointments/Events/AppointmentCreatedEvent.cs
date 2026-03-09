using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Appointments.Events;

public sealed record AppointmentCreatedEvent(Guid AppointmentId, Guid? SourceApiKeyId = null) : IDomainEvent;
