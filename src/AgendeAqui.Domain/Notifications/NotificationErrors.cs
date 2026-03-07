using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Notifications;

public static class NotificationErrors
{
    public static readonly Error InvalidTenant = new("Notification.InvalidTenant", "Tenant ID is required.");
    public static readonly Error InvalidAppointment = new("Notification.InvalidAppointment", "Appointment ID is required.");
    public static readonly Error InvalidRecipient = new("Notification.InvalidRecipient", "Recipient is required.");
    public static readonly Error InvalidTemplateName = new("Notification.InvalidTemplateName", "Template name is required.");
    public static readonly Error NotFound = new("Notification.NotFound", "Notification not found.");
}
