using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Schedules;

namespace AgendeAqui.Application.Schedules.UpdateSchedule;

public sealed class UpdateScheduleCommandHandler(
    IScheduleRepository scheduleRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateScheduleCommand>
{
    public async ValueTask<Result<Mediator.Unit>> Handle(
        UpdateScheduleCommand command,
        CancellationToken cancellationToken)
    {
        var schedule = await scheduleRepository.GetByIdAsync(command.ScheduleId, cancellationToken);
        if (schedule is null)
            return Result.Failure<Mediator.Unit>(ScheduleErrors.NotFound);

        var slotDuration = TimeSpan.FromMinutes(command.SlotDurationMinutes);

        var updateResult = schedule.Update(command.StartTime, command.EndTime, slotDuration);
        if (updateResult.IsFailure)
            return Result.Failure<Mediator.Unit>(updateResult.Error);

        scheduleRepository.Update(schedule);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Mediator.Unit.Value);
    }
}
