using AgendeAqui.Domain.Schedules;
using FluentAssertions;

namespace AgendeAqui.Domain.UnitTests.Schedules;

public class AbsenceTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _professionalId = Guid.NewGuid();

    [Fact]
    public void Create_FullDayAbsence_ShouldSucceedWithIsFullDayTrue()
    {
        var result = Absence.Create(_tenantId, _professionalId, DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            null, null, "Vacation");

        result.IsSuccess.Should().BeTrue();
        result.Value.IsFullDay.Should().BeTrue();
        result.Value.StartTime.Should().BeNull();
        result.Value.EndTime.Should().BeNull();
        result.Value.Reason.Should().Be("Vacation");
    }

    [Fact]
    public void Create_PartialAbsence_ShouldSucceedWithIsFullDayFalse()
    {
        var result = Absence.Create(_tenantId, _professionalId, DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            new TimeOnly(9, 0), new TimeOnly(12, 0), "Morning off");

        result.IsSuccess.Should().BeTrue();
        result.Value.IsFullDay.Should().BeFalse();
        result.Value.StartTime.Should().Be(new TimeOnly(9, 0));
        result.Value.EndTime.Should().Be(new TimeOnly(12, 0));
    }

    [Fact]
    public void Create_WithStartAfterEnd_ShouldFail()
    {
        var result = Absence.Create(_tenantId, _professionalId, DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            new TimeOnly(14, 0), new TimeOnly(9, 0), "Invalid");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AbsenceErrors.InvalidTimeRange);
    }

    [Fact]
    public void Create_WithOnlyStartTime_ShouldFail()
    {
        var result = Absence.Create(_tenantId, _professionalId, DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            new TimeOnly(9, 0), null, "Partial");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AbsenceErrors.InvalidTimeRange);
    }

    [Fact]
    public void Create_WithOnlyEndTime_ShouldFail()
    {
        var result = Absence.Create(_tenantId, _professionalId, DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            null, new TimeOnly(12, 0), "Partial");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AbsenceErrors.InvalidTimeRange);
    }

    [Fact]
    public void Create_WithEmptyProfessionalId_ShouldFail()
    {
        var result = Absence.Create(_tenantId, Guid.Empty, DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            null, null, "Vacation");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AbsenceErrors.InvalidProfessional);
    }

    [Fact]
    public void Blocks_FullDayAbsence_ShouldReturnTrueForAnySlot()
    {
        var absence = Absence.Create(_tenantId, _professionalId, DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            null, null, "Vacation").Value;

        absence.Blocks(new TimeOnly(9, 0), new TimeOnly(10, 0)).Should().BeTrue();
        absence.Blocks(new TimeOnly(14, 0), new TimeOnly(15, 0)).Should().BeTrue();
    }

    [Fact]
    public void Blocks_PartialAbsence_ShouldReturnTrueForOverlappingSlot()
    {
        var absence = Absence.Create(_tenantId, _professionalId, DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            new TimeOnly(9, 0), new TimeOnly(12, 0), "Morning off").Value;

        // Fully inside
        absence.Blocks(new TimeOnly(10, 0), new TimeOnly(11, 0)).Should().BeTrue();
        // Partially overlapping
        absence.Blocks(new TimeOnly(11, 30), new TimeOnly(12, 30)).Should().BeTrue();
    }

    [Fact]
    public void Blocks_PartialAbsence_ShouldReturnFalseForNonOverlappingSlot()
    {
        var absence = Absence.Create(_tenantId, _professionalId, DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            new TimeOnly(9, 0), new TimeOnly(12, 0), "Morning off").Value;

        // Completely after
        absence.Blocks(new TimeOnly(12, 0), new TimeOnly(13, 0)).Should().BeFalse();
        // Completely before
        absence.Blocks(new TimeOnly(7, 0), new TimeOnly(9, 0)).Should().BeFalse();
    }
}
