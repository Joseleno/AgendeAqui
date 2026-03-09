using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Schedules;

namespace AgendeAqui.Application.Schedules.CreateSchedule;

public sealed class CreateScheduleCommandHandler(
    IScheduleRepository scheduleRepository,
    IProfessionalRepository professionalRepository,
    IUnitOfWork unitOfWork,
    ITenantProvider tenantProvider) : ICommandHandler<CreateScheduleCommand, Guid>
{
    public async ValueTask<Result<Guid>> Handle(
        CreateScheduleCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.GetTenantId();

        var professional = await professionalRepository.GetByIdAsync(command.ProfessionalId, cancellationToken);
        if (professional is null)
            return Result.Failure<Guid>(ScheduleErrors.ProfessionalNotFound);
        if (!professional.IsActive)
            return Result.Failure<Guid>(ScheduleErrors.ProfessionalInactive);

        var existing = await scheduleRepository.GetByProfessionalAndDayAsync(
            command.ProfessionalId, command.DayOfWeek, cancellationToken);
        if (existing is not null)
            return Result.Failure<Guid>(ScheduleErrors.DuplicateDay);

        var slotDuration = TimeSpan.FromMinutes(command.SlotDurationMinutes);

        var scheduleResult = Schedule.Create(
            tenantId,
            command.ProfessionalId,
            command.DayOfWeek,
            command.StartTime,
            command.EndTime,
            slotDuration);

        if (scheduleResult.IsFailure)
            return Result.Failure<Guid>(scheduleResult.Error);

        var schedule = scheduleResult.Value;
        await scheduleRepository.AddAsync(schedule, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(schedule.Id);
    }
}
