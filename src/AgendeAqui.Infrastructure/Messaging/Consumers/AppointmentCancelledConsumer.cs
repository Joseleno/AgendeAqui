using AgendeAqui.Application.Abstractions.Notifications;
using AgendeAqui.Application.Appointments.IntegrationEvents;
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
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        await notificationService.SendAppointmentCancelledAsync(Guid.Empty, message.AppointmentId, message.Reason, ct);
    }
}
