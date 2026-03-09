namespace AgendeAqui.Application.Abstractions.RealTime;

public interface IAppointmentHubClient
{
    Task AppointmentCreated(AppointmentNotification notification);
    Task AppointmentCancelled(AppointmentCancelledNotification notification);
    Task AppointmentRescheduled(AppointmentRescheduledNotification notification);
}

public sealed record AppointmentNotification(
    Guid AppointmentId,
    Guid TenantId,
    DateTime OccurredAt);

public sealed record AppointmentCancelledNotification(
    Guid AppointmentId,
    Guid TenantId,
    string Reason,
    DateTime OccurredAt);

public sealed record AppointmentRescheduledNotification(
    Guid AppointmentId,
    Guid TenantId,
    DateOnly NewDate,
    DateTime OccurredAt);
