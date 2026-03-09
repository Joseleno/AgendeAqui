using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Appointments.IntegrationEvents;

public sealed record AppointmentCancelledIntegrationEvent(
    Guid Id,
    DateTime OccurredAt,
    Guid TenantId,
    Guid AppointmentId,
    string Reason) : IntegrationEvent(Id, OccurredAt);
