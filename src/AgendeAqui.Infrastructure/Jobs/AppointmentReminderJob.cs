using AgendeAqui.Application.Abstractions.Notifications;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AgendeAqui.Infrastructure.Jobs;

internal sealed class AppointmentReminderJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AppointmentReminderJob> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    public AppointmentReminderJob(
        IServiceScopeFactory scopeFactory,
        ILogger<AppointmentReminderJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing appointment reminders");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ProcessRemindersAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var appointmentRepository = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(24));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var appointments = await appointmentRepository.GetByDateRangeAsync(today, tomorrow, ct);

        _logger.LogInformation("Found {Count} appointments for reminders", appointments.Count);

        foreach (var appointment in appointments)
        {
            if (appointment.Status == AppointmentStatus.Scheduled ||
                appointment.Status == AppointmentStatus.Confirmed)
            {
                try
                {
                    await notificationService.SendAppointmentReminderAsync(
                        appointment.TenantId, appointment.Id, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send reminder for appointment {AppointmentId}", appointment.Id);
                }
            }
        }
    }
}
