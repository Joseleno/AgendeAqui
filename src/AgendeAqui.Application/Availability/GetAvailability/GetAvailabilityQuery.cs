using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Availability.GetAvailability;

public sealed record GetAvailabilityQuery(
    Guid ProfessionalId,
    DateOnly Date,
    Guid ServiceId) : IQuery<AvailabilityResponse>;
