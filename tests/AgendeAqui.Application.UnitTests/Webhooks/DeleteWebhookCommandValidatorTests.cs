using AgendeAqui.Application.Webhooks.DeleteWebhook;
using FluentValidation.TestHelper;

namespace AgendeAqui.Application.UnitTests.Webhooks;

public class DeleteWebhookCommandValidatorTests
{
    private readonly DeleteWebhookCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidWebhookId_ShouldHaveNoErrors()
    {
        var command = new DeleteWebhookCommand(Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyWebhookId_ShouldHaveError()
    {
        var command = new DeleteWebhookCommand(Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.WebhookId);
    }
}
