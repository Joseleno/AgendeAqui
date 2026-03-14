using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Absences.ListAbsences;

public sealed record ListAbsencesQuery(
    Guid ProfessionalId,
    DateOnly From,
    DateOnly To) : IQuery<IReadOnlyList<AbsenceResponse>>;
