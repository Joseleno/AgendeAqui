using AgendeAqui.Application.Webhooks.UpdateWebhook;
using FluentValidation.TestHelper;

namespace AgendeAqui.Application.UnitTests.Webhooks;

public class UpdateWebhookCommandValidatorTests
{
    private readonly UpdateWebhookCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        var command = new UpdateWebhookCommand(Guid.NewGuid(), "https://test.com/hook", ["appointment.created"], true);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyWebhookId_ShouldHaveError()
    {
        var command = new UpdateWebhookCommand(Guid.Empty, "https://test.com", ["event"], true);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.WebhookId);
    }

    [Fact]
    public void Validate_WithEmptyUrl_ShouldHaveError()
    {
        var command = new UpdateWebhookCommand(Guid.NewGuid(), "", ["event"], true);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Url);
    }

    [Fact]
    public void Validate_WithInvalidUrl_ShouldHaveError()
    {
        var command = new UpdateWebhookCommand(Guid.NewGuid(), "not-a-url", ["event"], true);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Url);
    }

    [Fact]
    public void Validate_WithUrlExceedingMaxLength_ShouldHaveError()
    {
        var longUrl = "https://test.com/" + new string('a', 2040);
        var command = new UpdateWebhookCommand(Guid.NewGuid(), longUrl, ["event"], true);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Url);
    }

    [Fact]
    public void Validate_WithEmptyEvents_ShouldHaveError()
    {
        var command = new UpdateWebhookCommand(Guid.NewGuid(), "https://test.com", [], true);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Events);
    }

    [Fact]
    public void Validate_WithHttpUrl_ShouldNotHaveErrors()
    {
        var command = new UpdateWebhookCommand(Guid.NewGuid(), "http://test.com/hook", ["appointment.created"], false);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
