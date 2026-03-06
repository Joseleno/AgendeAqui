using AgendeAqui.Domain.ValueObjects;
using FluentAssertions;

namespace AgendeAqui.Domain.UnitTests.ValueObjects;

public class TimeSlotTests
{
    [Fact]
    public void Create_WithValidTimes_ShouldSucceed()
    {
        var start = new TimeOnly(9, 0);
        var end = new TimeOnly(10, 0);

        var result = TimeSlot.Create(start, end);

        result.IsSuccess.Should().BeTrue();
        result.Value.Start.Should().Be(start);
        result.Value.End.Should().Be(end);
    }

    [Fact]
    public void Create_WhenStartEqualsEnd_ShouldFail()
    {
        var time = new TimeOnly(9, 0);

        var result = TimeSlot.Create(time, time);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TimeSlot.InvalidTimeSlot);
    }

    [Fact]
    public void Create_WhenStartAfterEnd_ShouldFail()
    {
        var start = new TimeOnly(11, 0);
        var end = new TimeOnly(9, 0);

        var result = TimeSlot.Create(start, end);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TimeSlot.InvalidTimeSlot);
    }

    [Fact]
    public void Duration_ShouldReflectDifferenceBetweenStartAndEnd()
    {
        var slot = TimeSlot.Create(new TimeOnly(9, 0), new TimeOnly(10, 30)).Value;

        slot.Duration.Should().Be(TimeSpan.FromMinutes(90));
    }

    [Fact]
    public void Overlaps_WhenSlotsOverlap_ShouldReturnTrue()
    {
        var slot1 = TimeSlot.Create(new TimeOnly(9, 0), new TimeOnly(11, 0)).Value;
        var slot2 = TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(12, 0)).Value;

        slot1.Overlaps(slot2).Should().BeTrue();
        slot2.Overlaps(slot1).Should().BeTrue();
    }

    [Fact]
    public void Overlaps_WhenSlotsAreContiguous_ShouldReturnFalse()
    {
        var slot1 = TimeSlot.Create(new TimeOnly(9, 0), new TimeOnly(10, 0)).Value;
        var slot2 = TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)).Value;

        slot1.Overlaps(slot2).Should().BeFalse();
        slot2.Overlaps(slot1).Should().BeFalse();
    }

    [Fact]
    public void Overlaps_WhenSlotsDoNotOverlap_ShouldReturnFalse()
    {
        var slot1 = TimeSlot.Create(new TimeOnly(9, 0), new TimeOnly(10, 0)).Value;
        var slot2 = TimeSlot.Create(new TimeOnly(11, 0), new TimeOnly(12, 0)).Value;

        slot1.Overlaps(slot2).Should().BeFalse();
    }

    [Fact]
    public void Overlaps_WhenOneSlotContainsOther_ShouldReturnTrue()
    {
        var outer = TimeSlot.Create(new TimeOnly(9, 0), new TimeOnly(12, 0)).Value;
        var inner = TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)).Value;

        outer.Overlaps(inner).Should().BeTrue();
        inner.Overlaps(outer).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithSameStartAndEnd_ShouldBeEqual()
    {
        var slot1 = TimeSlot.Create(new TimeOnly(9, 0), new TimeOnly(10, 0)).Value;
        var slot2 = TimeSlot.Create(new TimeOnly(9, 0), new TimeOnly(10, 0)).Value;

        slot1.Should().Be(slot2);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedTimeRange()
    {
        var slot = TimeSlot.Create(new TimeOnly(9, 0), new TimeOnly(10, 30)).Value;

        slot.ToString().Should().Be("09:00 - 10:30");
    }
}
