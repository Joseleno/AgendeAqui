using AgendeAqui.Application.Abstractions.Notifications;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AgendeAqui.Infrastructure.Jobs;

internal sealed class AppointmentReminderJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AppointmentReminderJob> _logger;
    private readonly ReminderSettings _settings;

    public AppointmentReminderJob(
        IServiceScopeFactory scopeFactory,
        ILogger<AppointmentReminderJob> logger,
        IOptions<ReminderSettings> settings)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _settings = settings.Value;
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

            await Task.Delay(TimeSpan.FromMinutes(_settings.IntervalMinutes), stoppingToken);
        }
    }

    internal async Task ProcessRemindersAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var appointmentRepository = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var localNow = DateTime.UtcNow.AddHours(_settings.TimezoneOffsetHours);
        var today = DateOnly.FromDateTime(localNow);
        var reminderDate = DateOnly.FromDateTime(localNow.AddHours(_settings.HoursBeforeAppointment));

        var appointments = await appointmentRepository.GetByDateRangeAsync(today, reminderDate, ct);

        _logger.LogInformation(
            "Found {Count} appointments for reminders (range: {From} to {To})",
            appointments.Count, today, reminderDate);

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
