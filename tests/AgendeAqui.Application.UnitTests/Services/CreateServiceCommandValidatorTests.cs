using AgendeAqui.Application.Services.CreateService;
using FluentValidation.TestHelper;

namespace AgendeAqui.Application.UnitTests.Services;

public class CreateServiceCommandValidatorTests
{
    private readonly CreateServiceCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldHaveNoErrors()
    {
        var command = new CreateServiceCommand("Haircut", 30, 50m);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyName_ShouldHaveError(string? name)
    {
        var command = new CreateServiceCommand(name!, 30, 50m);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithNameExceeding200Chars_ShouldHaveError()
    {
        var command = new CreateServiceCommand(new string('A', 201), 30, 50m);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithZeroOrNegativeDuration_ShouldHaveError(int duration)
    {
        var command = new CreateServiceCommand("Haircut", duration, 50m);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DurationMinutes);
    }

    [Fact]
    public void Validate_WithDurationOver480Minutes_ShouldHaveError()
    {
        var command = new CreateServiceCommand("Haircut", 481, 50m);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DurationMinutes);
    }

    [Fact]
    public void Validate_WithDurationAt480Minutes_ShouldHaveNoErrors()
    {
        var command = new CreateServiceCommand("Haircut", 480, 50m);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.DurationMinutes);
    }

    [Fact]
    public void Validate_WithNegativePrice_ShouldHaveError()
    {
        var command = new CreateServiceCommand("Haircut", 30, -1m);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Validate_WithZeroPrice_ShouldHaveNoErrors()
    {
        var command = new CreateServiceCommand("Free Consultation", 15, 0m);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Price);
    }
}
