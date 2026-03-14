using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Availability.GetAvailableProfessionals;

public sealed record GetAvailableProfessionalsQuery(
    Guid ServiceId,
    DateOnly Date) : IQuery<List<ProfessionalAvailabilityResponse>>;
