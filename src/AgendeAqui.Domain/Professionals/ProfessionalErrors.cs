using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Professionals;

public static class ProfessionalErrors
{
    public static readonly Error NotFound = new("Professional.NotFound", "Professional not found.");
    public static readonly Error InvalidName = new("Professional.InvalidName", "Professional name is required.");
}
