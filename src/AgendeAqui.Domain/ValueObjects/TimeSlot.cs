using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.ValueObjects;

public sealed class TimeSlot : ValueObject
{
    public static readonly Error InvalidTimeSlot = new("TimeSlot.Invalid", "Start time must be earlier than end time.");

    public TimeOnly Start { get; }
    public TimeOnly End { get; }
    public TimeSpan Duration => End - Start;

    private TimeSlot(TimeOnly start, TimeOnly end)
    {
        Start = start;
        End = end;
    }

    public static Result<TimeSlot> Create(TimeOnly start, TimeOnly end)
    {
        if (start >= end)
            return Result.Failure<TimeSlot>(InvalidTimeSlot);

        return Result.Success(new TimeSlot(start, end));
    }

    public bool Overlaps(TimeSlot other) =>
        Start < other.End && End > other.Start;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Start;
        yield return End;
    }

    public override string ToString() => $"{Start:HH:mm} - {End:HH:mm}";
}
