using AgendeAqui.Application.Schedules.UpdateSchedule;
using FluentValidation.TestHelper;

namespace AgendeAqui.Application.UnitTests.Schedules;

public class UpdateScheduleCommandValidatorTests
{
    private readonly UpdateScheduleCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldHaveNoErrors()
    {
        var command = new UpdateScheduleCommand(Guid.NewGuid(), new TimeOnly(8, 0), new TimeOnly(18, 0), 30);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyScheduleId_ShouldHaveError()
    {
        var command = new UpdateScheduleCommand(Guid.Empty, new TimeOnly(8, 0), new TimeOnly(18, 0), 30);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ScheduleId);
    }

    [Fact]
    public void Validate_WithZeroSlotDuration_ShouldHaveError()
    {
        var command = new UpdateScheduleCommand(Guid.NewGuid(), new TimeOnly(8, 0), new TimeOnly(18, 0), 0);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SlotDurationMinutes);
    }

    [Fact]
    public void Validate_WithNegativeSlotDuration_ShouldHaveError()
    {
        var command = new UpdateScheduleCommand(Guid.NewGuid(), new TimeOnly(8, 0), new TimeOnly(18, 0), -5);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SlotDurationMinutes);
    }

    [Fact]
    public void Validate_WithStartTimeAfterEndTime_ShouldHaveError()
    {
        var command = new UpdateScheduleCommand(Guid.NewGuid(), new TimeOnly(18, 0), new TimeOnly(8, 0), 30);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.StartTime);
    }

    [Fact]
    public void Validate_WithStartTimeEqualToEndTime_ShouldHaveError()
    {
        var command = new UpdateScheduleCommand(Guid.NewGuid(), new TimeOnly(8, 0), new TimeOnly(8, 0), 30);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.StartTime);
    }

    [Fact]
    public void Validate_WithSlotDurationExceedingMax_ShouldHaveError()
    {
        var command = new UpdateScheduleCommand(Guid.NewGuid(), new TimeOnly(8, 0), new TimeOnly(18, 0), 721);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SlotDurationMinutes);
    }
}
