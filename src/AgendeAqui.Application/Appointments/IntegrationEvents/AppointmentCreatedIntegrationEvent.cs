using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Appointments.IntegrationEvents;

public sealed record AppointmentCreatedIntegrationEvent(
    Guid Id,
    DateTime OccurredAt,
    Guid TenantId,
    Guid AppointmentId) : IntegrationEvent(Id, OccurredAt);
