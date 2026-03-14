using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Absences.CreateAbsence;

public sealed record CreateAbsenceCommand(
    Guid ProfessionalId,
    DateOnly Date,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    string? Reason) : ICommand<Guid>;
