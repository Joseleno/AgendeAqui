using AgendeAqui.Application.Schedules.DeactivateSchedule;
using FluentValidation.TestHelper;

namespace AgendeAqui.Application.UnitTests.Schedules;

public class DeactivateScheduleCommandValidatorTests
{
    private readonly DeactivateScheduleCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldHaveNoErrors()
    {
        var command = new DeactivateScheduleCommand(Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyScheduleId_ShouldHaveError()
    {
        var command = new DeactivateScheduleCommand(Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ScheduleId);
    }
}
