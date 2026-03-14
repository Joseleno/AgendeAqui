using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Professionals;
using AgendeAqui.Domain.Schedules;

namespace AgendeAqui.Application.Absences.CreateAbsence;

public sealed class CreateAbsenceCommandHandler(
    IAbsenceRepository absenceRepository,
    IProfessionalRepository professionalRepository,
    IUnitOfWork unitOfWork,
    ITenantProvider tenantProvider) : ICommandHandler<CreateAbsenceCommand, Guid>
{
    public async ValueTask<Result<Guid>> Handle(
        CreateAbsenceCommand command,
        CancellationToken cancellationToken)
    {
        var professional = await professionalRepository.GetByIdAsync(command.ProfessionalId, cancellationToken);
        if (professional is null)
            return Result.Failure<Guid>(ProfessionalErrors.NotFound);

        var tenantId = tenantProvider.GetTenantId();

        var absenceResult = Absence.Create(
            tenantId,
            command.ProfessionalId,
            command.Date,
            command.StartTime,
            command.EndTime,
            command.Reason);

        if (absenceResult.IsFailure)
            return Result.Failure<Guid>(absenceResult.Error);

        var newAbsence = absenceResult.Value;
        var existingAbsences = await absenceRepository.GetByProfessionalAndDateAsync(
            command.ProfessionalId, command.Date, cancellationToken);

        foreach (var existing in existingAbsences)
        {
            if (existing.IsFullDay)
                return Result.Failure<Guid>(AbsenceErrors.Overlap);

            if (newAbsence.IsFullDay)
                return Result.Failure<Guid>(AbsenceErrors.Overlap);

            if (newAbsence.StartTime.HasValue && newAbsence.EndTime.HasValue
                && existing.Blocks(newAbsence.StartTime.Value, newAbsence.EndTime.Value))
                return Result.Failure<Guid>(AbsenceErrors.Overlap);
        }

        await absenceRepository.AddAsync(newAbsence, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(absenceResult.Value.Id);
    }
}
