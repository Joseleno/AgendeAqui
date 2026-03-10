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
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing appointment reminders");
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(_settings.IntervalMinutes), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
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

        AppointmentStatus[] eligibleStatuses = [AppointmentStatus.Scheduled, AppointmentStatus.Confirmed];
        var eligibleAppointments = await appointmentRepository.GetByDateRangeAndStatusesAsync(
            today, reminderDate, eligibleStatuses, ct);

        _logger.LogInformation(
            "Found {Count} eligible appointments for reminders (range: {From} to {To})",
            eligibleAppointments.Count, today, reminderDate);

        if (eligibleAppointments.Count > 0)
        {
            try
            {
                await notificationService.SendAppointmentRemindersAsync(eligibleAppointments, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send batch reminders for {Count} appointments", eligibleAppointments.Count);
            }
        }
    }
}
