using AgendeAqui.Application.Auth.Login;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace AgendeAqui.Application.UnitTests.Auth;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Validate_WithEmptyEmail_Fails()
    {
        var command = new LoginCommand("", "password123");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithInvalidEmail_Fails()
    {
        var command = new LoginCommand("not-an-email", "password123");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithEmptyPassword_Fails()
    {
        var command = new LoginCommand("user@example.com", "");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WithValidInput_Passes()
    {
        var command = new LoginCommand("user@example.com", "SecurePass123!");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
