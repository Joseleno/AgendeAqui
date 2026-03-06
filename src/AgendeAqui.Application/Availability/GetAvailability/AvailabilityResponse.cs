namespace AgendeAqui.Application.Availability.GetAvailability;

public sealed record AvailabilityResponse(
    DateOnly Date,
    Guid ProfessionalId,
    IReadOnlyList<AvailableSlot> Slots);

public sealed record AvailableSlot(TimeOnly Start, TimeOnly End);
