using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.ValueObjects;

namespace AgendeAqui.Application.Appointments.RescheduleAppointment;

public sealed class RescheduleAppointmentCommandHandler(
    IAppointmentRepository appointmentRepository,
    IServiceRepository serviceRepository,
    IScheduleRepository scheduleRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<RescheduleAppointmentCommand>
{
    public async ValueTask<Result<Mediator.Unit>> Handle(
        RescheduleAppointmentCommand command,
        CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.GetByIdAsync(command.AppointmentId, cancellationToken);
        if (appointment is null)
            return Result.Failure<Mediator.Unit>(AppointmentErrors.NotFound);

        var service = await serviceRepository.GetByIdAsync(appointment.ServiceId, cancellationToken);
        if (service is null)
            return Result.Failure<Mediator.Unit>(AppointmentErrors.ServiceNotFound);

        var endTime = command.NewStartTime.Add(service.Duration);
        var timeSlotResult = TimeSlot.Create(command.NewStartTime, endTime);
        if (timeSlotResult.IsFailure)
            return Result.Failure<Mediator.Unit>(timeSlotResult.Error);

        var schedule = await scheduleRepository.GetByProfessionalAndDayAsync(
            appointment.ProfessionalId, command.NewDate.DayOfWeek, cancellationToken);
        if (schedule is null)
            return Result.Failure<Mediator.Unit>(AppointmentErrors.NoSchedule);

        if (command.NewStartTime < schedule.StartTime || endTime > schedule.EndTime)
            return Result.Failure<Mediator.Unit>(AppointmentErrors.OutsideSchedule);

        var hasConflict = await appointmentRepository.HasConflictAsync(
            appointment.ProfessionalId, command.NewDate, command.NewStartTime, endTime,
            excludeAppointmentId: appointment.Id, ct: cancellationToken);
        if (hasConflict)
            return Result.Failure<Mediator.Unit>(AppointmentErrors.Conflict);

        var result = appointment.Reschedule(command.NewDate, timeSlotResult.Value);
        if (result.IsFailure)
            return Result.Failure<Mediator.Unit>(result.Error);

        appointmentRepository.Update(appointment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Mediator.Unit.Value);
    }
}
