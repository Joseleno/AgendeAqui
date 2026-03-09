using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Appointments.IntegrationEvents;

public sealed record AppointmentRescheduledIntegrationEvent(
    Guid Id,
    DateTime OccurredAt,
    Guid TenantId,
    Guid AppointmentId,
    DateOnly NewDate) : IntegrationEvent(Id, OccurredAt);
