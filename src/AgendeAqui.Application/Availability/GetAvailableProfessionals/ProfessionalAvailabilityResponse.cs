namespace AgendeAqui.Application.Availability.GetAvailableProfessionals;

public sealed record ProfessionalAvailabilityResponse(
    Guid ProfessionalId,
    string Name,
    string? Specialty,
    int AvailableSlots);
