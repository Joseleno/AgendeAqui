using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments.Events;
using AgendeAqui.Domain.InAppNotifications;
using Microsoft.Extensions.Logging;

namespace AgendeAqui.Application.InAppNotifications.Events;

public sealed class AppointmentCancelledNotificationHandler(
    IAppointmentRepository appointmentRepository,
    IUserRepository userRepository,
    IInAppNotificationRepository notificationRepository,
    IUnitOfWork unitOfWork,
    ITenantProvider tenantProvider,
    ILogger<AppointmentCancelledNotificationHandler> logger) : IDomainEventHandler<AppointmentCancelledEvent>
{
    public async ValueTask Handle(AppointmentCancelledEvent notification, CancellationToken cancellationToken)
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
            "Appointment cancelled",
            $"An appointment on {appointment.Date:yyyy-MM-dd} has been cancelled. Reason: {notification.Reason}",
            InAppNotificationType.AppointmentCancelled,
            appointment.Id);

        await notificationRepository.AddAsync(inAppNotification, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
