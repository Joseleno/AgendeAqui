using AgendeAqui.Application.Webhooks.RegisterWebhook;
using FluentValidation.TestHelper;

namespace AgendeAqui.Application.UnitTests.Webhooks;

public class RegisterWebhookCommandValidatorTests
{
    private readonly RegisterWebhookCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidData_ShouldHaveNoErrors()
    {
        var command = new RegisterWebhookCommand(
            "https://example.com/webhook",
            new string('s', 32),
            ["appointment.created", "appointment.cancelled"]);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WithEmptyUrl_ShouldHaveError(string? url)
    {
        var command = new RegisterWebhookCommand(url!, new string('s', 32), ["appointment.created"]);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Url);
    }

    [Fact]
    public async Task Validate_WithUrlExceedingMaxLength_ShouldHaveError()
    {
        var longUrl = "https://example.com/" + new string('a', 2030);
        var command = new RegisterWebhookCommand(longUrl, new string('s', 32), ["appointment.created"]);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Url);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_WithEmptySecret_ShouldHaveError(string? secret)
    {
        var command = new RegisterWebhookCommand("https://example.com/webhook", secret!, ["appointment.created"]);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Secret);
    }

    [Fact]
    public async Task Validate_WithShortSecret_ShouldHaveError()
    {
        var command = new RegisterWebhookCommand(
            "https://example.com/webhook",
            new string('s', 31),
            ["appointment.created"]);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Secret);
    }

    [Fact]
    public async Task Validate_WithEmptyEvents_ShouldHaveError()
    {
        var command = new RegisterWebhookCommand("https://example.com/webhook", new string('s', 32), []);

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Events);
    }
}
