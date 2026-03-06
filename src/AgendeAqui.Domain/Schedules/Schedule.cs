using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.ValueObjects;

namespace AgendeAqui.Domain.Schedules;

public sealed class Schedule : AggregateRoot
{
    public static readonly Error InvalidSchedule = new("Schedule.Invalid", "End time must be after start time.");
    public static readonly Error InvalidSlotDuration = new("Schedule.InvalidSlotDuration", "Slot duration must be positive and fit within the schedule window.");

    private readonly List<BreakPeriod> _breaks = [];

    public Guid ProfessionalId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public TimeSpan SlotDuration { get; private set; }
    public bool IsActive { get; private set; }
    public IReadOnlyList<BreakPeriod> Breaks => _breaks.AsReadOnly();

    private Schedule() { }

    public static Result<Schedule> Create(
        Guid tenantId,
        Guid professionalId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        TimeSpan slotDuration)
    {
        if (startTime >= endTime)
            return Result.Failure<Schedule>(InvalidSchedule);

        if (slotDuration <= TimeSpan.Zero || slotDuration > (endTime - startTime))
            return Result.Failure<Schedule>(InvalidSlotDuration);

        var schedule = new Schedule
        {
            TenantId = tenantId,
            ProfessionalId = professionalId,
            DayOfWeek = dayOfWeek,
            StartTime = startTime,
            EndTime = endTime,
            SlotDuration = slotDuration,
            IsActive = true
        };

        return Result.Success(schedule);
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsAvailableAt(TimeOnly time)
    {
        if (!IsActive || time < StartTime || time >= EndTime)
            return false;

        return !_breaks.Any(b => time >= b.BreakStart && time < b.BreakEnd);
    }

    public IReadOnlyList<TimeSlot> GetAvailableSlots(TimeSpan serviceDuration)
    {
        var slots = new List<TimeSlot>();
        var current = StartTime;

        while (current.Add(serviceDuration) <= EndTime)
        {
            var slotEnd = current.Add(serviceDuration);
            var slotResult = TimeSlot.Create(current, slotEnd);

            if (slotResult.IsSuccess && !IsBlockedByBreak(current, slotEnd))
                slots.Add(slotResult.Value);

            current = current.Add(SlotDuration);
        }

        return slots.AsReadOnly();
    }

    public void AddBreak(TimeOnly breakStart, TimeOnly breakEnd)
    {
        _breaks.Add(new BreakPeriod(breakStart, breakEnd));
        UpdatedAt = DateTime.UtcNow;
    }

    private bool IsBlockedByBreak(TimeOnly start, TimeOnly end) =>
        _breaks.Any(b => start < b.BreakEnd && end > b.BreakStart);
}

public sealed record BreakPeriod(TimeOnly BreakStart, TimeOnly BreakEnd);
