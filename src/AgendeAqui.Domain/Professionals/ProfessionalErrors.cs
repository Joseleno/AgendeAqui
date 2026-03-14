using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Professionals;

public static class ProfessionalErrors
{
    public static readonly Error NotFound = new("Professional.NotFound", "Professional not found.");
    public static readonly Error InvalidName = new("Professional.InvalidName", "Professional name is required.");
    public static readonly Error InvalidSpecialty = new("Professional.InvalidSpecialty", "Specialty is required when provided.");
    public static readonly Error SpecialtyTooLong = new("Professional.SpecialtyTooLong", $"Specialty must be at most {Specialty.MaxLength} characters.");
    public static readonly Error ServiceAlreadyLinked = new("Professional.ServiceAlreadyLinked", "Service is already linked to this professional.");
    public static readonly Error ServiceNotLinked = new("Professional.ServiceNotLinked", "Service is not linked to this professional.");
}
