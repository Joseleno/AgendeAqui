using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Schedules;

public static class AbsenceErrors
{
    public static readonly Error NotFound = new("Absence.NotFound", "Absence not found.");
    public static readonly Error InvalidProfessional = new("Absence.InvalidProfessional", "A valid professional is required.");
    public static readonly Error InvalidTimeRange = new("Absence.InvalidTimeRange", "Start time must be before end time. Provide both or neither for a full-day absence.");
    public static readonly Error Overlap = new("Absence.Conflict", "An absence already exists for this professional on this date and time range.");
    public static readonly Error NotAuthorized = new("Absence.NotAuthorized", "You are not authorized to manage this absence.");
}
