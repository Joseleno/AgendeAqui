using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments.Events;
using AgendeAqui.Domain.InAppNotifications;
using Microsoft.Extensions.Logging;

namespace AgendeAqui.Application.InAppNotifications.Events;

public sealed class AppointmentNoShowNotificationHandler(
    IAppointmentRepository appointmentRepository,
    IUserRepository userRepository,
    IInAppNotificationRepository notificationRepository,
    IUnitOfWork unitOfWork,
    ITenantProvider tenantProvider,
    ILogger<AppointmentNoShowNotificationHandler> logger) : IDomainEventHandler<AppointmentNoShowEvent>
{
    public async ValueTask Handle(AppointmentNoShowEvent notification, CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.GetByIdAsync(notification.AppointmentId, cancellationToken);
        if (appointment is null)
        {
            logger.LogWarning("Appointment {AppointmentId} not found for in-app notification", notification.AppointmentId);
            return;
        }

        var user = await userRepository.GetByProfessionalIdAsync(appointment.ProfessionalId, cancellationToken);
        if (user is null)
        {
            logger.LogWarning("No user linked to professional {ProfessionalId} for in-app notification", appointment.ProfessionalId);
            return;
        }

        var tenantId = tenantProvider.GetTenantId();

        var inAppNotification = InAppNotification.Create(
            tenantId,
            user.Id,
            "Client no-show",
            $"The client did not show up for the appointment on {appointment.Date:yyyy-MM-dd}.",
            InAppNotificationType.AppointmentNoShow,
            appointment.Id);

        await notificationRepository.AddAsync(inAppNotification, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
