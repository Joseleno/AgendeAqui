using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Schedules;

namespace AgendeAqui.Application.Schedules.DeactivateSchedule;

public sealed class DeactivateScheduleCommandHandler(
    IScheduleRepository scheduleRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeactivateScheduleCommand>
{
    public async ValueTask<Result<Mediator.Unit>> Handle(
        DeactivateScheduleCommand command,
        CancellationToken cancellationToken)
    {
        var schedule = await scheduleRepository.GetByIdAsync(command.ScheduleId, cancellationToken);
        if (schedule is null)
            return Result.Failure<Mediator.Unit>(ScheduleErrors.NotFound);

        schedule.Deactivate();

        scheduleRepository.Update(schedule);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Mediator.Unit.Value);
    }
}
