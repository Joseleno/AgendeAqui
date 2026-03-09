using AgendeAqui.Application.Abstractions.Notifications;
using AgendeAqui.Application.Appointments.IntegrationEvents;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Infrastructure.Observability;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgendeAqui.Infrastructure.Messaging.Consumers;

internal sealed class AppointmentCreatedConsumer : RabbitMqConsumerBase<AppointmentCreatedIntegrationEvent>
{
    public AppointmentCreatedConsumer(
        RabbitMqConnection connection,
        IServiceScopeFactory scopeFactory,
        ILogger<AppointmentCreatedConsumer> logger,
        CustomMetrics metrics)
        : base(connection, scopeFactory, logger, "appointment.created", metrics) { }

    protected override async Task ProcessAsync(
        AppointmentCreatedIntegrationEvent message,
        IServiceScope scope,
        CancellationToken ct)
    {
        var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantProvider>();
        tenantProvider.SetTenantId(message.TenantId);

        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        await notificationService.SendAppointmentCreatedAsync(message.TenantId, message.AppointmentId, ct);
    }
}
