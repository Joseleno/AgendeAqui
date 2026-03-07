using AgendeAqui.Domain.Schedules;
using AgendeAqui.Domain.ValueObjects;
using FluentAssertions;

namespace AgendeAqui.Domain.UnitTests.Schedules;

public class ScheduleTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid ProfessionalId = Guid.NewGuid();

    private static Schedule CreateSchedule(
        TimeOnly? start = null,
        TimeOnly? end = null,
        TimeSpan? slot = null) =>
        Schedule.Create(
            TenantId,
            ProfessionalId,
            DayOfWeek.Monday,
            start ?? new TimeOnly(8, 0),
            end ?? new TimeOnly(18, 0),
            slot ?? TimeSpan.FromMinutes(30)).Value;

    [Fact]
    public void Create_WithValidParameters_ShouldSucceed()
    {
        var result = Schedule.Create(
            TenantId, ProfessionalId, DayOfWeek.Monday,
            new TimeOnly(8, 0), new TimeOnly(18, 0), TimeSpan.FromMinutes(30));

        result.IsSuccess.Should().BeTrue();
        result.Value.TenantId.Should().Be(TenantId);
        result.Value.ProfessionalId.Should().Be(ProfessionalId);
        result.Value.DayOfWeek.Should().Be(DayOfWeek.Monday);
        result.Value.StartTime.Should().Be(new TimeOnly(8, 0));
        result.Value.EndTime.Should().Be(new TimeOnly(18, 0));
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithStartTimeAfterEndTime_ShouldReturnInvalidSchedule()
    {
        var result = Schedule.Create(
            TenantId, ProfessionalId, DayOfWeek.Monday,
            new TimeOnly(18, 0), new TimeOnly(8, 0), TimeSpan.FromMinutes(30));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ScheduleErrors.InvalidSchedule);
    }

    [Fact]
    public void Create_WithStartTimeEqualToEndTime_ShouldReturnInvalidSchedule()
    {
        var result = Schedule.Create(
            TenantId, ProfessionalId, DayOfWeek.Monday,
            new TimeOnly(8, 0), new TimeOnly(8, 0), TimeSpan.FromMinutes(30));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ScheduleErrors.InvalidSchedule);
    }

    [Fact]
    public void Create_WithZeroSlotDuration_ShouldReturnInvalidSlotDuration()
    {
        var result = Schedule.Create(
            TenantId, ProfessionalId, DayOfWeek.Monday,
            new TimeOnly(8, 0), new TimeOnly(18, 0), TimeSpan.Zero);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ScheduleErrors.InvalidSlotDuration);
    }

    [Fact]
    public void Create_WithSlotDurationLargerThanWindow_ShouldReturnInvalidSlotDuration()
    {
        var result = Schedule.Create(
            TenantId, ProfessionalId, DayOfWeek.Monday,
            new TimeOnly(8, 0), new TimeOnly(9, 0), TimeSpan.FromHours(2));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ScheduleErrors.InvalidSlotDuration);
    }

    [Fact]
    public void Update_WithValidParameters_ShouldSucceedAndUpdateTimestamp()
    {
        var schedule = CreateSchedule();
        var before = DateTime.UtcNow.AddSeconds(-1);

        var result = schedule.Update(new TimeOnly(9, 0), new TimeOnly(17, 0), TimeSpan.FromMinutes(60));

        result.IsSuccess.Should().BeTrue();
        schedule.StartTime.Should().Be(new TimeOnly(9, 0));
        schedule.EndTime.Should().Be(new TimeOnly(17, 0));
        schedule.SlotDuration.Should().Be(TimeSpan.FromMinutes(60));
        schedule.UpdatedAt.Should().BeAfter(before);
    }

    [Fact]
    public void Update_WithStartTimeAfterEndTime_ShouldReturnInvalidSchedule()
    {
        var schedule = CreateSchedule();

        var result = schedule.Update(new TimeOnly(18, 0), new TimeOnly(8, 0), TimeSpan.FromMinutes(30));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ScheduleErrors.InvalidSchedule);
    }

    [Fact]
    public void Update_WithStartTimeEqualToEndTime_ShouldReturnInvalidSchedule()
    {
        var schedule = CreateSchedule();

        var result = schedule.Update(new TimeOnly(8, 0), new TimeOnly(8, 0), TimeSpan.FromMinutes(30));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ScheduleErrors.InvalidSchedule);
    }

    [Fact]
    public void Update_WithZeroSlotDuration_ShouldReturnInvalidSlotDuration()
    {
        var schedule = CreateSchedule();

        var result = schedule.Update(new TimeOnly(8, 0), new TimeOnly(18, 0), TimeSpan.Zero);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ScheduleErrors.InvalidSlotDuration);
    }

    [Fact]
    public void Update_WithSlotDurationLargerThanWindow_ShouldReturnInvalidSlotDuration()
    {
        var schedule = CreateSchedule(new TimeOnly(8, 0), new TimeOnly(9, 0));

        var result = schedule.Update(new TimeOnly(8, 0), new TimeOnly(9, 0), TimeSpan.FromHours(2));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ScheduleErrors.InvalidSlotDuration);
    }

    [Fact]
    public void Activate_WhenInactive_ShouldSetIsActiveToTrueAndRefreshTimestamp()
    {
        var schedule = CreateSchedule();
        schedule.Deactivate();
        var timestampAfterDeactivate = schedule.UpdatedAt;

        schedule.Activate();

        schedule.IsActive.Should().BeTrue();
        schedule.UpdatedAt.Should().NotBeNull();
        schedule.UpdatedAt.Should().BeOnOrAfter(timestampAfterDeactivate!.Value);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalseAndUpdateTimestamp()
    {
        var schedule = CreateSchedule();
        var before = DateTime.UtcNow.AddSeconds(-1);

        schedule.Deactivate();

        schedule.IsActive.Should().BeFalse();
        schedule.UpdatedAt.Should().NotBeNull();
        schedule.UpdatedAt.Should().BeAfter(before);
    }

    [Fact]
    public void AddBreak_WithValidRange_ShouldAddBreakPeriod()
    {
        var schedule = CreateSchedule();

        var result = schedule.AddBreak(new TimeOnly(12, 0), new TimeOnly(13, 0));

        result.IsSuccess.Should().BeTrue();
        schedule.Breaks.Should().HaveCount(1);
        schedule.Breaks[0].BreakStart.Should().Be(new TimeOnly(12, 0));
        schedule.Breaks[0].BreakEnd.Should().Be(new TimeOnly(13, 0));
    }

    [Fact]
    public void AddBreak_WithStartAfterEnd_ShouldReturnInvalidBreak()
    {
        var schedule = CreateSchedule();

        var result = schedule.AddBreak(new TimeOnly(13, 0), new TimeOnly(12, 0));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ScheduleErrors.InvalidBreak);
    }

    [Fact]
    public void AddBreak_WithStartEqualToEnd_ShouldReturnInvalidBreak()
    {
        var schedule = CreateSchedule();

        var result = schedule.AddBreak(new TimeOnly(12, 0), new TimeOnly(12, 0));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ScheduleErrors.InvalidBreak);
    }

    [Fact]
    public void AddBreak_WithBreakEndExceedingEndTime_ShouldReturnInvalidBreak()
    {
        var schedule = CreateSchedule();

        var result = schedule.AddBreak(new TimeOnly(17, 0), new TimeOnly(19, 0));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ScheduleErrors.InvalidBreak);
    }

    [Fact]
    public void AddBreak_OutsideScheduleWindow_ShouldReturnInvalidBreak()
    {
        var schedule = CreateSchedule();

        var result = schedule.AddBreak(new TimeOnly(7, 0), new TimeOnly(8, 30));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ScheduleErrors.InvalidBreak);
    }

    [Fact]
    public void AddBreak_WithValidRange_ShouldUpdateTimestamp()
    {
        var schedule = CreateSchedule();
        var before = DateTime.UtcNow.AddSeconds(-1);

        schedule.AddBreak(new TimeOnly(12, 0), new TimeOnly(13, 0));

        schedule.UpdatedAt.Should().NotBeNull();
        schedule.UpdatedAt.Should().BeAfter(before);
    }

    [Fact]
    public void GetAvailableSlots_WithNoBreaks_ShouldReturnAllSlots()
    {
        var schedule = CreateSchedule(
            new TimeOnly(8, 0), new TimeOnly(10, 0), TimeSpan.FromMinutes(60));

        var slots = schedule.GetAvailableSlots(TimeSpan.FromMinutes(60));

        slots.Should().HaveCount(2);
        slots[0].Start.Should().Be(new TimeOnly(8, 0));
        slots[1].Start.Should().Be(new TimeOnly(9, 0));
    }

    [Fact]
    public void GetAvailableSlots_WithBreakCoveringSlot_ShouldExcludeBlockedSlot()
    {
        var schedule = CreateSchedule(
            new TimeOnly(8, 0), new TimeOnly(10, 0), TimeSpan.FromMinutes(60));
        schedule.AddBreak(new TimeOnly(8, 0), new TimeOnly(9, 0));

        var slots = schedule.GetAvailableSlots(TimeSpan.FromMinutes(60));

        slots.Should().HaveCount(1);
        slots[0].Start.Should().Be(new TimeOnly(9, 0));
    }

    [Fact]
    public void GetAvailableSlots_WithPartialBreakOverlap_ShouldExcludeBlockedSlot()
    {
        var schedule = CreateSchedule(
            new TimeOnly(8, 0), new TimeOnly(10, 0), TimeSpan.FromMinutes(60));
        schedule.AddBreak(new TimeOnly(8, 30), new TimeOnly(9, 0));

        var slots = schedule.GetAvailableSlots(TimeSpan.FromMinutes(60));

        slots.Should().HaveCount(1);
        slots[0].Start.Should().Be(new TimeOnly(9, 0));
    }

    [Fact]
    public void GetAvailableSlots_WithServiceDurationSmallerThanSlot_ShouldReturnCorrectSlots()
    {
        var schedule = CreateSchedule(
            new TimeOnly(8, 0), new TimeOnly(10, 0), TimeSpan.FromMinutes(30));

        var slots = schedule.GetAvailableSlots(TimeSpan.FromMinutes(60));

        slots.Should().HaveCount(3);
        slots[0].Start.Should().Be(new TimeOnly(8, 0));
        slots[1].Start.Should().Be(new TimeOnly(8, 30));
        slots[2].Start.Should().Be(new TimeOnly(9, 0));
    }

    [Fact]
    public void GetAvailableSlots_WithZeroServiceDuration_ShouldReturnEmpty()
    {
        var schedule = CreateSchedule(
            new TimeOnly(8, 0), new TimeOnly(10, 0), TimeSpan.FromMinutes(60));

        var slots = schedule.GetAvailableSlots(TimeSpan.Zero);

        slots.Should().BeEmpty();
    }

    [Fact]
    public void GetAvailableSlots_WhenInactive_ShouldReturnEmpty()
    {
        var schedule = CreateSchedule(
            new TimeOnly(8, 0), new TimeOnly(10, 0), TimeSpan.FromMinutes(60));
        schedule.Deactivate();

        var slots = schedule.GetAvailableSlots(TimeSpan.FromMinutes(60));

        slots.Should().BeEmpty();
    }

    [Fact]
    public void IsAvailableAt_WithActiveScheduleAndTimeInRange_ShouldReturnTrue()
    {
        var schedule = CreateSchedule();

        var available = schedule.IsAvailableAt(new TimeOnly(10, 0));

        available.Should().BeTrue();
    }

    [Fact]
    public void IsAvailableAt_WithInactiveSchedule_ShouldReturnFalse()
    {
        var schedule = CreateSchedule();
        schedule.Deactivate();

        var available = schedule.IsAvailableAt(new TimeOnly(10, 0));

        available.Should().BeFalse();
    }

    [Fact]
    public void IsAvailableAt_WithTimeBeforeStart_ShouldReturnFalse()
    {
        var schedule = CreateSchedule();

        var available = schedule.IsAvailableAt(new TimeOnly(7, 0));

        available.Should().BeFalse();
    }

    [Fact]
    public void IsAvailableAt_WithTimeAtEndTime_ShouldReturnFalse()
    {
        var schedule = CreateSchedule();

        var available = schedule.IsAvailableAt(new TimeOnly(18, 0));

        available.Should().BeFalse();
    }

    [Fact]
    public void IsAvailableAt_WithTimeDuringBreak_ShouldReturnFalse()
    {
        var schedule = CreateSchedule();
        schedule.AddBreak(new TimeOnly(12, 0), new TimeOnly(13, 0));

        var available = schedule.IsAvailableAt(new TimeOnly(12, 30));

        available.Should().BeFalse();
    }
}
