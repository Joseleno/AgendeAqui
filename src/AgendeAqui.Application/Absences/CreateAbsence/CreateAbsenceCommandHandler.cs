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

        await absenceRepository.AddAsync(absenceResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(absenceResult.Value.Id);
    }
}
