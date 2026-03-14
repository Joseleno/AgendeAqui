using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments;
using AgendeAqui.Domain.Common;

namespace AgendeAqui.Application.Appointments.CancelAppointment;

public sealed class CancelAppointmentCommandHandler(
    IAppointmentRepository appointmentRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<CancelAppointmentCommand>
{
    public async ValueTask<Result<Mediator.Unit>> Handle(
        CancelAppointmentCommand command,
        CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.GetByIdAsync(command.AppointmentId, cancellationToken);
        if (appointment is null)
            return Result.Failure<Mediator.Unit>(AppointmentErrors.NotFound);

        // Enforce ownership: Client can only cancel their own appointments
        if (currentUser.Role == "Client" && appointment.ClientId != currentUser.ClientId)
            return Result.Failure<Mediator.Unit>(AppointmentErrors.NotAuthorized);

        // Enforce ownership: Professional can only cancel their own appointments
        if (currentUser.Role == "Professional" && appointment.ProfessionalId != currentUser.ProfessionalId)
            return Result.Failure<Mediator.Unit>(AppointmentErrors.NotAuthorized);

        var result = appointment.Cancel(command.Reason);
        if (result.IsFailure)
            return Result.Failure<Mediator.Unit>(result.Error);

        appointmentRepository.Update(appointment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Mediator.Unit.Value);
    }
}
