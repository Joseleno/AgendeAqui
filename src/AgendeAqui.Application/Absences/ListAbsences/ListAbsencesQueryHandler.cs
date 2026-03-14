using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;

namespace AgendeAqui.Application.Absences.ListAbsences;

public sealed class ListAbsencesQueryHandler(
    IAbsenceRepository absenceRepository) : IQueryHandler<ListAbsencesQuery, IReadOnlyList<AbsenceResponse>>
{
    public async ValueTask<Result<IReadOnlyList<AbsenceResponse>>> Handle(
        ListAbsencesQuery query,
        CancellationToken cancellationToken)
    {
        var absences = await absenceRepository.GetByProfessionalAndDateRangeAsync(
            query.ProfessionalId, query.From, query.To, cancellationToken);

        var response = absences.Select(a => new AbsenceResponse(
            a.Id,
            a.ProfessionalId,
            a.Date,
            a.StartTime,
            a.EndTime,
            a.Reason,
            a.IsFullDay)).ToList();

        return Result.Success<IReadOnlyList<AbsenceResponse>>(response);
    }
}
