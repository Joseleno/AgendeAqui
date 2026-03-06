using AgendeAqui.Application.Professionals.CreateProfessional;
using FluentValidation.TestHelper;

namespace AgendeAqui.Application.UnitTests.Professionals;

public class CreateProfessionalCommandValidatorTests
{
    private readonly CreateProfessionalCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldHaveNoErrors()
    {
        var command = new CreateProfessionalCommand("John Doe", "john@example.com", "5511987654321");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyName_ShouldHaveError(string? name)
    {
        var command = new CreateProfessionalCommand(name!, "john@example.com", "5511987654321");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithNameExceeding200Chars_ShouldHaveError()
    {
        var command = new CreateProfessionalCommand(new string('A', 201), "john@example.com", "5511987654321");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyEmail_ShouldHaveError(string? email)
    {
        var command = new CreateProfessionalCommand("John", email!, "5511987654321");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithEmailExceeding320Chars_ShouldHaveError()
    {
        var command = new CreateProfessionalCommand("John", new string('a', 321), "5511987654321");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyPhone_ShouldHaveError(string? phone)
    {
        var command = new CreateProfessionalCommand("John", "john@example.com", phone!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }

    [Fact]
    public void Validate_WithPhoneExceeding20Chars_ShouldHaveError()
    {
        var command = new CreateProfessionalCommand("John", "john@example.com", new string('1', 21));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }
}
