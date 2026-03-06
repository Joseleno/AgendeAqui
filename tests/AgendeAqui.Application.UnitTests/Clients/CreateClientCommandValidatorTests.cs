using AgendeAqui.Application.Clients.CreateClient;
using FluentValidation.TestHelper;

namespace AgendeAqui.Application.UnitTests.Clients;

public class CreateClientCommandValidatorTests
{
    private readonly CreateClientCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldHaveNoErrors()
    {
        var command = new CreateClientCommand("Maria Silva", "maria@example.com", "5511987654321");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyName_ShouldHaveError(string? name)
    {
        var command = new CreateClientCommand(name!, "maria@example.com", "5511987654321");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithNameExceeding200Chars_ShouldHaveError()
    {
        var command = new CreateClientCommand(new string('A', 201), "maria@example.com", "5511987654321");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyEmail_ShouldHaveError(string? email)
    {
        var command = new CreateClientCommand("Maria", email!, "5511987654321");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithEmailExceeding320Chars_ShouldHaveError()
    {
        var command = new CreateClientCommand("Maria", new string('a', 321), "5511987654321");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyPhone_ShouldHaveError(string? phone)
    {
        var command = new CreateClientCommand("Maria", "maria@example.com", phone!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }

    [Fact]
    public void Validate_WithPhoneExceeding20Chars_ShouldHaveError()
    {
        var command = new CreateClientCommand("Maria", "maria@example.com", new string('1', 21));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }
}
