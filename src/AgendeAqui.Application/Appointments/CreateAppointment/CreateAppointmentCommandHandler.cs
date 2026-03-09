using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.ValueObjects;

namespace AgendeAqui.Application.Appointments.CreateAppointment;

public sealed class CreateAppointmentCommandHandler(
    IAppointmentRepository appointmentRepository,
    IProfessionalRepository professionalRepository,
    IServiceRepository serviceRepository,
    IClientRepository clientRepository,
    IScheduleRepository scheduleRepository,
    IUnitOfWork unitOfWork,
    ITenantProvider tenantProvider,
    ICurrentUser currentUser) : ICommandHandler<CreateAppointmentCommand, Guid>
{
    public async ValueTask<Result<Guid>> Handle(
        CreateAppointmentCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.GetTenantId();

        var professional = await professionalRepository.GetByIdAsync(command.ProfessionalId, cancellationToken);
        if (professional is null)
            return Result.Failure<Guid>(AppointmentErrors.ProfessionalNotFound);
        if (!professional.IsActive)
            return Result.Failure<Guid>(AppointmentErrors.ProfessionalInactive);

        var service = await serviceRepository.GetByIdAsync(command.ServiceId, cancellationToken);
        if (service is null)
            return Result.Failure<Guid>(AppointmentErrors.ServiceNotFound);
        if (!service.IsActive)
            return Result.Failure<Guid>(AppointmentErrors.ServiceInactive);

        var client = await clientRepository.GetByIdAsync(command.ClientId, cancellationToken);
        if (client is null)
            return Result.Failure<Guid>(AppointmentErrors.ClientNotFound);

        var endTime = command.StartTime.Add(service.Duration);
        var timeSlotResult = TimeSlot.Create(command.StartTime, endTime);
        if (timeSlotResult.IsFailure)
            return Result.Failure<Guid>(timeSlotResult.Error);

        var schedule = await scheduleRepository.GetByProfessionalAndDayAsync(
            command.ProfessionalId, command.Date.DayOfWeek, cancellationToken);
        if (schedule is null)
            return Result.Failure<Guid>(AppointmentErrors.NoSchedule);

        if (command.StartTime < schedule.StartTime || endTime > schedule.EndTime)
            return Result.Failure<Guid>(AppointmentErrors.OutsideSchedule);

        var hasConflict = await appointmentRepository.HasConflictAsync(
            command.ProfessionalId, command.Date, command.StartTime, endTime, ct: cancellationToken);
        if (hasConflict)
            return Result.Failure<Guid>(AppointmentErrors.Conflict);

        Guid? sourceApiKeyId = currentUser.Role.Equals("ApiKey", StringComparison.OrdinalIgnoreCase)
            ? currentUser.UserId
            : null;

        var appointmentResult = Appointment.Create(
            tenantId,
            command.ProfessionalId,
            command.ServiceId,
            command.ClientId,
            command.Date,
            timeSlotResult.Value,
            command.Notes,
            sourceApiKeyId);

        if (appointmentResult.IsFailure)
            return Result.Failure<Guid>(appointmentResult.Error);

        var appointment = appointmentResult.Value;

        if (!string.IsNullOrWhiteSpace(command.ExternalId))
            appointment.SetExternalId(command.ExternalId);

        await appointmentRepository.AddAsync(appointment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(appointment.Id);
    }
}
