using FluentValidation;

namespace AgendeAqui.Application.Webhooks.DeleteWebhook;

public sealed class DeleteWebhookCommandValidator : AbstractValidator<DeleteWebhookCommand>
{
    public DeleteWebhookCommandValidator()
    {
        RuleFor(x => x.WebhookId)
            .NotEmpty();
    }
}
