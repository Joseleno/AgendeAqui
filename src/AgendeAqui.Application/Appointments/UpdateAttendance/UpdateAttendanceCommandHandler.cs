using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments;
using AgendeAqui.Domain.Common;

namespace AgendeAqui.Application.Appointments.UpdateAttendance;

public sealed class UpdateAttendanceCommandHandler(
    IAppointmentRepository appointmentRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateAttendanceCommand>
{
    public async ValueTask<Result<Mediator.Unit>> Handle(
        UpdateAttendanceCommand command,
        CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.GetByIdAsync(command.AppointmentId, cancellationToken);
        if (appointment is null)
            return Result.Failure<Mediator.Unit>(AppointmentErrors.NotFound);

        var result = command.Action switch
        {
            AttendanceAction.Confirm => appointment.Confirm(),
            AttendanceAction.Start => appointment.Start(),
            AttendanceAction.Complete => appointment.Complete(),
            AttendanceAction.NoShow => appointment.MarkNoShow(),
            _ => Result.Failure(AppointmentErrors.InvalidTransition)
        };

        if (result.IsFailure)
            return Result.Failure<Mediator.Unit>(result.Error);

        appointmentRepository.Update(appointment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Mediator.Unit.Value);
    }
}
