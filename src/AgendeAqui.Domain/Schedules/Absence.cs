using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Schedules;

public sealed class Absence : TenantEntity
{
    public Guid ProfessionalId { get; private set; }
    public DateOnly Date { get; private set; }
    public TimeOnly? StartTime { get; private set; }
    public TimeOnly? EndTime { get; private set; }
    public string? Reason { get; private set; }

    /// <summary>
    /// True when StartTime and EndTime are null — the professional is absent the entire day.
    /// </summary>
    public bool IsFullDay => StartTime is null && EndTime is null;

    private Absence() { }

    public static Result<Absence> Create(
        Guid tenantId,
        Guid professionalId,
        DateOnly date,
        TimeOnly? startTime,
        TimeOnly? endTime,
        string? reason)
    {
        if (professionalId == Guid.Empty)
            return Result.Failure<Absence>(AbsenceErrors.InvalidProfessional);

        // Both must be provided or both null (full-day)
        if (startTime.HasValue != endTime.HasValue)
            return Result.Failure<Absence>(AbsenceErrors.InvalidTimeRange);

        if (startTime.HasValue && endTime.HasValue && startTime.Value >= endTime.Value)
            return Result.Failure<Absence>(AbsenceErrors.InvalidTimeRange);

        var absence = new Absence
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            TenantId = tenantId,
            ProfessionalId = professionalId,
            Date = date,
            StartTime = startTime,
            EndTime = endTime,
            Reason = reason?.Trim()
        };

        return Result.Success(absence);
    }

    /// <summary>
    /// Returns true if the given time slot overlaps with this absence.
    /// </summary>
    public bool Blocks(TimeOnly slotStart, TimeOnly slotEnd)
    {
        if (IsFullDay)
            return true;

        return slotStart < EndTime!.Value && slotEnd > StartTime!.Value;
    }
}
