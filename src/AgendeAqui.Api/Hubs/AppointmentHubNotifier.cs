using AgendeAqui.Application.Abstractions.RealTime;
using Microsoft.AspNetCore.SignalR;

namespace AgendeAqui.Api.Hubs;

internal sealed class AppointmentHubNotifier(
    IHubContext<AppointmentHub, IAppointmentHubClient> hubContext,
    ILogger<AppointmentHubNotifier> logger) : IAppointmentHubNotifier
{
    public async Task NotifyAppointmentCreatedAsync(Guid tenantId, Guid appointmentId, CancellationToken ct)
    {
        logger.LogInformation(
            "SignalR: AppointmentCreated {AppointmentId} for tenant {TenantId}",
            appointmentId,
            tenantId);

        await hubContext.Clients
            .Group($"tenant:{tenantId}")
            .AppointmentCreated(new AppointmentNotification(appointmentId, tenantId, DateTime.UtcNow));
    }

    public async Task NotifyAppointmentCancelledAsync(Guid tenantId, Guid appointmentId, string reason, CancellationToken ct)
    {
        logger.LogInformation(
            "SignalR: AppointmentCancelled {AppointmentId} for tenant {TenantId}",
            appointmentId,
            tenantId);

        await hubContext.Clients
            .Group($"tenant:{tenantId}")
            .AppointmentCancelled(new AppointmentCancelledNotification(appointmentId, tenantId, reason, DateTime.UtcNow));
    }

    public async Task NotifyAppointmentRescheduledAsync(Guid tenantId, Guid appointmentId, DateOnly newDate, CancellationToken ct)
    {
        logger.LogInformation(
            "SignalR: AppointmentRescheduled {AppointmentId} for tenant {TenantId}",
            appointmentId,
            tenantId);

        await hubContext.Clients
            .Group($"tenant:{tenantId}")
            .AppointmentRescheduled(new AppointmentRescheduledNotification(appointmentId, tenantId, newDate, DateTime.UtcNow));
    }
}
