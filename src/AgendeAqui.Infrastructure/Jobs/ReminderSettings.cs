namespace AgendeAqui.Infrastructure.Jobs;

public sealed class ReminderSettings
{
    public const string SectionName = "Reminders";

    /// <summary>
    /// How often the job runs to check for upcoming appointments (in minutes).
    /// </summary>
    public int IntervalMinutes { get; init; } = 60;

    /// <summary>
    /// How many hours before the appointment to send the reminder.
    /// </summary>
    public int HoursBeforeAppointment { get; init; } = 24;

    /// <summary>
    /// UTC offset for the target timezone (e.g., -3 for Brazil BRT).
    /// Used to correctly calculate "tomorrow" relative to the tenant's local time.
    /// </summary>
    public int TimezoneOffsetHours { get; init; } = -3;
}
