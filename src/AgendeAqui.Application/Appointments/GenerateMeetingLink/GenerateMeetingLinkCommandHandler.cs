using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments;
using AgendeAqui.Domain.Common;

namespace AgendeAqui.Application.Appointments.GenerateMeetingLink;

public sealed class GenerateMeetingLinkCommandHandler(
    IAppointmentRepository appointmentRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<GenerateMeetingLinkCommand, string>
{
    public async ValueTask<Result<string>> Handle(
        GenerateMeetingLinkCommand command,
        CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.GetByIdAsync(command.AppointmentId, cancellationToken);
        if (appointment is null)
            return Result.Failure<string>(AppointmentErrors.NotFound);

        appointment.SetTeleconsultation(true);
        appointment.GenerateMeetingLink();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(appointment.MeetingUrl!);
    }
}
