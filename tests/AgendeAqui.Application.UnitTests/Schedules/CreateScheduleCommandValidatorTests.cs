using AgendeAqui.Application.Schedules.CreateSchedule;
using FluentValidation.TestHelper;

namespace AgendeAqui.Application.UnitTests.Schedules;

public class CreateScheduleCommandValidatorTests
{
    private readonly CreateScheduleCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldHaveNoErrors()
    {
        var command = new CreateScheduleCommand(
            Guid.NewGuid(), DayOfWeek.Monday,
            new TimeOnly(8, 0), new TimeOnly(18, 0), 30);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyProfessionalId_ShouldHaveError()
    {
        var command = new CreateScheduleCommand(
            Guid.Empty, DayOfWeek.Monday,
            new TimeOnly(8, 0), new TimeOnly(18, 0), 30);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ProfessionalId);
    }

    [Fact]
    public void Validate_WithZeroSlotDuration_ShouldHaveError()
    {
        var command = new CreateScheduleCommand(
            Guid.NewGuid(), DayOfWeek.Monday,
            new TimeOnly(8, 0), new TimeOnly(18, 0), 0);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SlotDurationMinutes);
    }

    [Fact]
    public void Validate_WithStartTimeAfterEndTime_ShouldHaveError()
    {
        var command = new CreateScheduleCommand(
            Guid.NewGuid(), DayOfWeek.Monday,
            new TimeOnly(18, 0), new TimeOnly(8, 0), 30);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.StartTime);
    }

    [Fact]
    public void Validate_WithStartTimeEqualToEndTime_ShouldHaveError()
    {
        var command = new CreateScheduleCommand(
            Guid.NewGuid(), DayOfWeek.Monday,
            new TimeOnly(8, 0), new TimeOnly(8, 0), 30);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.StartTime);
    }

    [Fact]
    public void Validate_WithNegativeSlotDuration_ShouldHaveError()
    {
        var command = new CreateScheduleCommand(
            Guid.NewGuid(), DayOfWeek.Monday,
            new TimeOnly(8, 0), new TimeOnly(18, 0), -5);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SlotDurationMinutes);
    }

    [Fact]
    public void Validate_WithSlotDurationExceedingMax_ShouldHaveError()
    {
        var command = new CreateScheduleCommand(
            Guid.NewGuid(), DayOfWeek.Monday,
            new TimeOnly(8, 0), new TimeOnly(18, 0), 721);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SlotDurationMinutes);
    }

    [Fact]
    public void Validate_WithInvalidDayOfWeek_ShouldHaveError()
    {
        var command = new CreateScheduleCommand(
            Guid.NewGuid(), (DayOfWeek)7,
            new TimeOnly(8, 0), new TimeOnly(18, 0), 30);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DayOfWeek);
    }
}
