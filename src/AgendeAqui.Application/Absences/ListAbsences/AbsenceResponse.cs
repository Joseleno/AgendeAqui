namespace AgendeAqui.Application.Absences.ListAbsences;

public sealed record AbsenceResponse(
    Guid Id,
    Guid ProfessionalId,
    DateOnly Date,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    string? Reason,
    bool IsFullDay);
