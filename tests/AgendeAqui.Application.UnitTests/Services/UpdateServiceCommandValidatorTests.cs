using AgendeAqui.Application.Services.UpdateService;
using FluentValidation.TestHelper;

namespace AgendeAqui.Application.UnitTests.Services;

public class UpdateServiceCommandValidatorTests
{
    private readonly UpdateServiceCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldHaveNoErrors()
    {
        var command = new UpdateServiceCommand(Guid.NewGuid(), "Haircut", 30, 50m);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyId_ShouldHaveError()
    {
        var command = new UpdateServiceCommand(Guid.Empty, "Haircut", 30, 50m);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ServiceId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyName_ShouldHaveError(string? name)
    {
        var command = new UpdateServiceCommand(Guid.NewGuid(), name!, 30, 50m);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithNameExceeding200Chars_ShouldHaveError()
    {
        var command = new UpdateServiceCommand(Guid.NewGuid(), new string('A', 201), 30, 50m);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithZeroOrNegativeDuration_ShouldHaveError(int duration)
    {
        var command = new UpdateServiceCommand(Guid.NewGuid(), "Haircut", duration, 50m);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DurationMinutes);
    }

    [Fact]
    public void Validate_WithDurationOver480Minutes_ShouldHaveError()
    {
        var command = new UpdateServiceCommand(Guid.NewGuid(), "Haircut", 481, 50m);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DurationMinutes);
    }

    [Fact]
    public void Validate_WithNegativePrice_ShouldHaveError()
    {
        var command = new UpdateServiceCommand(Guid.NewGuid(), "Haircut", 30, -1m);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Validate_WithZeroPrice_ShouldHaveNoErrors()
    {
        var command = new UpdateServiceCommand(Guid.NewGuid(), "Free Consultation", 15, 0m);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Price);
    }
}
