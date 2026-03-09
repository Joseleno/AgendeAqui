using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Appointments.Events;

public sealed record AppointmentCancelledEvent(Guid AppointmentId, string Reason, Guid? SourceApiKeyId = null) : IDomainEvent;
