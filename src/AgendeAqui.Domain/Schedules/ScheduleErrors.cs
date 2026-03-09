using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Schedules;

public static class ScheduleErrors
{
    public static readonly Error NotFound = new("Schedule.NotFound", "Schedule not found.");
    public static readonly Error DuplicateDay = new("Schedule.DuplicateDay", "A schedule already exists for this professional on this day.");
    public static readonly Error ProfessionalNotFound = new("Schedule.ProfessionalNotFound", "The specified professional was not found.");
    public static readonly Error ProfessionalInactive = new("Schedule.ProfessionalInactive", "The specified professional is not active.");
    public static readonly Error InvalidSchedule = new("Schedule.Invalid", "End time must be after start time.");
    public static readonly Error InvalidSlotDuration = new("Schedule.InvalidSlotDuration", "Slot duration must be positive and fit within the schedule window.");
    public static readonly Error InvalidBreak = new("Schedule.InvalidBreak", "Break start must be before break end and within the schedule window.");
}
