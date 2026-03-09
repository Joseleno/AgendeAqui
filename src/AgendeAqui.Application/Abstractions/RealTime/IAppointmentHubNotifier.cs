namespace AgendeAqui.Application.Abstractions.RealTime;

public interface IAppointmentHubNotifier
{
    Task NotifyAppointmentCreatedAsync(Guid tenantId, Guid appointmentId, CancellationToken ct = default);
    Task NotifyAppointmentCancelledAsync(Guid tenantId, Guid appointmentId, string reason, CancellationToken ct = default);
    Task NotifyAppointmentRescheduledAsync(Guid tenantId, Guid appointmentId, DateOnly newDate, CancellationToken ct = default);
}
