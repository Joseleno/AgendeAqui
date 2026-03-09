using AgendeAqui.Application.Abstractions.Notifications;
using AgendeAqui.Application.Appointments.IntegrationEvents;
using AgendeAqui.Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgendeAqui.Infrastructure.Messaging.Consumers;

internal sealed class AppointmentRescheduledConsumer : RabbitMqConsumerBase<AppointmentRescheduledIntegrationEvent>
{
    public AppointmentRescheduledConsumer(
        RabbitMqConnection connection,
        IServiceScopeFactory scopeFactory,
        ILogger<AppointmentRescheduledConsumer> logger,
        Observability.CustomMetrics metrics)
        : base(connection, scopeFactory, logger, "appointment.rescheduled", metrics) { }

    protected override async Task ProcessAsync(
        AppointmentRescheduledIntegrationEvent message,
        IServiceScope scope,
        CancellationToken ct)
    {
        var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantProvider>();
        tenantProvider.SetTenantId(message.TenantId);

        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        await notificationService.SendAppointmentRescheduledAsync(message.TenantId, message.AppointmentId, message.NewDate, ct);
    }
}
