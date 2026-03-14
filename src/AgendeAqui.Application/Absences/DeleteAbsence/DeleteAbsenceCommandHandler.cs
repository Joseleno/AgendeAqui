using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Schedules;

namespace AgendeAqui.Application.Absences.DeleteAbsence;

public sealed class DeleteAbsenceCommandHandler(
    IAbsenceRepository absenceRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteAbsenceCommand>
{
    public async ValueTask<Result<Mediator.Unit>> Handle(
        DeleteAbsenceCommand command,
        CancellationToken cancellationToken)
    {
        var absence = await absenceRepository.GetByIdAsync(command.AbsenceId, cancellationToken);
        if (absence is null)
            return Result.Failure<Mediator.Unit>(AbsenceErrors.NotFound);

        absenceRepository.Remove(absence);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Mediator.Unit.Value);
    }
}
