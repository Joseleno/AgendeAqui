using AgendeAqui.Application.Webhooks.UpdateWebhook;
using FluentValidation.TestHelper;

namespace AgendeAqui.Application.UnitTests.Webhooks;

public class UpdateWebhookCommandValidatorTests
{
    private readonly UpdateWebhookCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        var command = new UpdateWebhookCommand(Guid.NewGuid(), "https://example.com/hook", ["appointment.created"], true);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WithEmptyWebhookId_ShouldHaveError()
    {
        var command = new UpdateWebhookCommand(Guid.Empty, "https://example.com", ["event"], true);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.WebhookId);
    }

    [Fact]
    public async Task Validate_WithEmptyUrl_ShouldHaveError()
    {
        var command = new UpdateWebhookCommand(Guid.NewGuid(), "", ["event"], true);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Url);
    }

    [Fact]
    public async Task Validate_WithInvalidUrl_ShouldHaveError()
    {
        var command = new UpdateWebhookCommand(Guid.NewGuid(), "not-a-url", ["event"], true);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Url);
    }

    [Fact]
    public async Task Validate_WithUrlExceedingMaxLength_ShouldHaveError()
    {
        var longUrl = "https://example.com/" + new string('a', 2040);
        var command = new UpdateWebhookCommand(Guid.NewGuid(), longUrl, ["event"], true);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Url);
    }

    [Fact]
    public async Task Validate_WithEmptyEvents_ShouldHaveError()
    {
        var command = new UpdateWebhookCommand(Guid.NewGuid(), "https://example.com", [], true);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Events);
    }

    [Fact]
    public async Task Validate_WithHttpUrl_ShouldNotHaveErrors()
    {
        var command = new UpdateWebhookCommand(Guid.NewGuid(), "http://example.com/hook", ["appointment.created"], false);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
