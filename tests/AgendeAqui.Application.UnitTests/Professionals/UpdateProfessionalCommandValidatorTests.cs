using AgendeAqui.Application.Professionals.UpdateProfessional;
using FluentValidation.TestHelper;

namespace AgendeAqui.Application.UnitTests.Professionals;

public class UpdateProfessionalCommandValidatorTests
{
    private readonly UpdateProfessionalCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldHaveNoErrors()
    {
        var command = new UpdateProfessionalCommand(Guid.NewGuid(), "John Doe", "john@example.com", "5511987654321");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyId_ShouldHaveError()
    {
        var command = new UpdateProfessionalCommand(Guid.Empty, "John", "john@example.com", "5511987654321");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ProfessionalId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyName_ShouldHaveError(string? name)
    {
        var command = new UpdateProfessionalCommand(Guid.NewGuid(), name!, "john@example.com", "5511987654321");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithNameExceeding200Chars_ShouldHaveError()
    {
        var command = new UpdateProfessionalCommand(Guid.NewGuid(), new string('A', 201), "john@example.com", "5511987654321");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyEmail_ShouldHaveError(string? email)
    {
        var command = new UpdateProfessionalCommand(Guid.NewGuid(), "John", email!, "5511987654321");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithEmailExceeding320Chars_ShouldHaveError()
    {
        var command = new UpdateProfessionalCommand(Guid.NewGuid(), "John", new string('a', 321), "5511987654321");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyPhone_ShouldHaveError(string? phone)
    {
        var command = new UpdateProfessionalCommand(Guid.NewGuid(), "John", "john@example.com", phone!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }

    [Fact]
    public void Validate_WithPhoneExceeding20Chars_ShouldHaveError()
    {
        var command = new UpdateProfessionalCommand(Guid.NewGuid(), "John", "john@example.com", new string('1', 21));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }
}
