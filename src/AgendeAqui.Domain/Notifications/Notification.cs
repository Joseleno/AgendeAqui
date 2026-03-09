using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Notifications;

public sealed class Notification : TenantEntity
{
    public Guid AppointmentId { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public string Recipient { get; private set; } = default!;
    public string TemplateName { get; private set; } = default!;
    public NotificationStatus Status { get; private set; }
    public DateTime? SentAt { get; private set; }
    public string? ErrorMessage { get; private set; }

    private Notification() { }

    public static Result<Notification> Create(
        Guid tenantId,
        Guid appointmentId,
        NotificationChannel channel,
        string recipient,
        string templateName)
    {
        if (tenantId == Guid.Empty)
            return Result.Failure<Notification>(NotificationErrors.InvalidTenant);

        if (appointmentId == Guid.Empty)
            return Result.Failure<Notification>(NotificationErrors.InvalidAppointment);

        if (string.IsNullOrWhiteSpace(recipient))
            return Result.Failure<Notification>(NotificationErrors.InvalidRecipient);

        if (string.IsNullOrWhiteSpace(templateName))
            return Result.Failure<Notification>(NotificationErrors.InvalidTemplateName);

        return Result.Success(new Notification
        {
            TenantId = tenantId,
            AppointmentId = appointmentId,
            Channel = channel,
            Recipient = recipient,
            TemplateName = templateName,
            Status = NotificationStatus.Pending
        });
    }

    public void MarkAsSent()
    {
        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = NotificationStatus.Failed;
        ErrorMessage = errorMessage;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDelivered()
    {
        Status = NotificationStatus.Delivered;
        UpdatedAt = DateTime.UtcNow;
    }
}
