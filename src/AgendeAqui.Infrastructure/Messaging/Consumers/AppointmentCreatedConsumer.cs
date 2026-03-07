using AgendeAqui.Application.Abstractions.Notifications;
using AgendeAqui.Application.Appointments.IntegrationEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgendeAqui.Infrastructure.Messaging.Consumers;

internal sealed class AppointmentCreatedConsumer : RabbitMqConsumerBase<AppointmentCreatedIntegrationEvent>
{
    public AppointmentCreatedConsumer(
        RabbitMqConnection connection,
        IServiceScopeFactory scopeFactory,
        ILogger<AppointmentCreatedConsumer> logger)
        : base(connection, scopeFactory, logger, "appointment.created") { }

    protected override async Task ProcessAsync(
        AppointmentCreatedIntegrationEvent message,
        IServiceScope scope,
        CancellationToken ct)
    {
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        await notificationService.SendAppointmentCreatedAsync(Guid.Empty, message.AppointmentId, ct);
    }
}
