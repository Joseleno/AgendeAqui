using AgendeAqui.Application.Abstractions.Notifications;
using AgendeAqui.Application.Appointments.IntegrationEvents;
using AgendeAqui.Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgendeAqui.Infrastructure.Messaging.Consumers;

internal sealed class AppointmentCancelledConsumer : RabbitMqConsumerBase<AppointmentCancelledIntegrationEvent>
{
    public AppointmentCancelledConsumer(
        RabbitMqConnection connection,
        IServiceScopeFactory scopeFactory,
        ILogger<AppointmentCancelledConsumer> logger)
        : base(connection, scopeFactory, logger, "appointment.cancelled") { }

    protected override async Task ProcessAsync(
        AppointmentCancelledIntegrationEvent message,
        IServiceScope scope,
        CancellationToken ct)
    {
        var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantProvider>();
        tenantProvider.SetTenantId(message.TenantId);

        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        await notificationService.SendAppointmentCancelledAsync(message.TenantId, message.AppointmentId, message.Reason, ct);
    }
}
