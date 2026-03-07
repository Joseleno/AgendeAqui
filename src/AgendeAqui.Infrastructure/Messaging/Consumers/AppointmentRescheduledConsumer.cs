using AgendeAqui.Application.Abstractions.Notifications;
using AgendeAqui.Application.Appointments.IntegrationEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgendeAqui.Infrastructure.Messaging.Consumers;

internal sealed class AppointmentRescheduledConsumer : RabbitMqConsumerBase<AppointmentRescheduledIntegrationEvent>
{
    public AppointmentRescheduledConsumer(
        RabbitMqConnection connection,
        IServiceScopeFactory scopeFactory,
        ILogger<AppointmentRescheduledConsumer> logger)
        : base(connection, scopeFactory, logger, "appointment.rescheduled") { }

    protected override async Task ProcessAsync(
        AppointmentRescheduledIntegrationEvent message,
        IServiceScope scope,
        CancellationToken ct)
    {
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        await notificationService.SendAppointmentRescheduledAsync(Guid.Empty, message.AppointmentId, message.NewDate, ct);
    }
}
