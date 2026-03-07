using AgendeAqui.Application.Abstractions.Notifications;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Notifications;
using Microsoft.Extensions.Logging;

namespace AgendeAqui.Infrastructure.Notifications;

internal sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IWhatsAppClient _whatsAppClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepository,
        IAppointmentRepository appointmentRepository,
        IClientRepository clientRepository,
        IWhatsAppClient whatsAppClient,
        IUnitOfWork unitOfWork,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _appointmentRepository = appointmentRepository;
        _clientRepository = clientRepository;
        _whatsAppClient = whatsAppClient;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task SendAppointmentCreatedAsync(Guid tenantId, Guid appointmentId, CancellationToken ct = default)
    {
        await SendNotificationAsync(tenantId, appointmentId, "appointment_created", new Dictionary<string, string>(), ct);
    }

    public async Task SendAppointmentCancelledAsync(Guid tenantId, Guid appointmentId, string reason, CancellationToken ct = default)
    {
        var parameters = new Dictionary<string, string> { ["reason"] = reason };
        await SendNotificationAsync(tenantId, appointmentId, "appointment_cancelled", parameters, ct);
    }

    public async Task SendAppointmentRescheduledAsync(Guid tenantId, Guid appointmentId, DateOnly newDate, CancellationToken ct = default)
    {
        var parameters = new Dictionary<string, string> { ["new_date"] = newDate.ToString("dd/MM/yyyy") };
        await SendNotificationAsync(tenantId, appointmentId, "appointment_rescheduled", parameters, ct);
    }

    public async Task SendAppointmentReminderAsync(Guid tenantId, Guid appointmentId, CancellationToken ct = default)
    {
        await SendNotificationAsync(tenantId, appointmentId, "appointment_reminder", new Dictionary<string, string>(), ct);
    }

    private async Task SendNotificationAsync(
        Guid tenantId,
        Guid appointmentId,
        string templateName,
        Dictionary<string, string> parameters,
        CancellationToken ct)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, ct);
        if (appointment is null)
        {
            _logger.LogWarning("Appointment {AppointmentId} not found for notification", appointmentId);
            return;
        }

        var client = await _clientRepository.GetByIdAsync(appointment.ClientId, ct);
        if (client is null)
        {
            _logger.LogWarning("Client {ClientId} not found for notification", appointment.ClientId);
            return;
        }

        var phoneNumber = client.Phone.Value;
        var actualTenantId = tenantId == Guid.Empty ? appointment.TenantId : tenantId;

        var notificationResult = Notification.Create(
            actualTenantId,
            appointmentId,
            NotificationChannel.WhatsApp,
            phoneNumber,
            templateName);

        if (notificationResult.IsFailure)
        {
            _logger.LogWarning("Failed to create notification: {Error}", notificationResult.Error.Message);
            return;
        }

        var notification = notificationResult.Value;

        try
        {
            var sent = await _whatsAppClient.SendTemplateMessageAsync(phoneNumber, templateName, parameters, ct);

            if (sent)
                notification.MarkAsSent();
            else
                notification.MarkAsFailed("WhatsApp API returned false");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send WhatsApp notification for appointment {AppointmentId}", appointmentId);
            notification.MarkAsFailed(ex.Message);
        }

        await _notificationRepository.AddAsync(notification, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
