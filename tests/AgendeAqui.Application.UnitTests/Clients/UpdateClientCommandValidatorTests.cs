using AgendeAqui.Application.Clients.UpdateClient;
using FluentValidation.TestHelper;

namespace AgendeAqui.Application.UnitTests.Clients;

public class UpdateClientCommandValidatorTests
{
    private readonly UpdateClientCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldHaveNoErrors()
    {
        var command = new UpdateClientCommand(Guid.NewGuid(), "Maria Silva", "maria@example.com", "5511987654321");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyId_ShouldHaveError()
    {
        var command = new UpdateClientCommand(Guid.Empty, "Maria", "maria@example.com", "5511987654321");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ClientId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyName_ShouldHaveError(string? name)
    {
        var command = new UpdateClientCommand(Guid.NewGuid(), name!, "maria@example.com", "5511987654321");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithNameExceeding200Chars_ShouldHaveError()
    {
        var command = new UpdateClientCommand(Guid.NewGuid(), new string('A', 201), "maria@example.com", "5511987654321");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyEmail_ShouldHaveError(string? email)
    {
        var command = new UpdateClientCommand(Guid.NewGuid(), "Maria", email!, "5511987654321");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithEmailExceeding320Chars_ShouldHaveError()
    {
        var command = new UpdateClientCommand(Guid.NewGuid(), "Maria", new string('a', 321), "5511987654321");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyPhone_ShouldHaveError(string? phone)
    {
        var command = new UpdateClientCommand(Guid.NewGuid(), "Maria", "maria@example.com", phone!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }

    [Fact]
    public void Validate_WithPhoneExceeding20Chars_ShouldHaveError()
    {
        var command = new UpdateClientCommand(Guid.NewGuid(), "Maria", "maria@example.com", new string('1', 21));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }
}
