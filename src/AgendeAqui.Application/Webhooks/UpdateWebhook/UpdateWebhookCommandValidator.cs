using FluentValidation;

namespace AgendeAqui.Application.Webhooks.UpdateWebhook;

public sealed class UpdateWebhookCommandValidator : AbstractValidator<UpdateWebhookCommand>
{
    public UpdateWebhookCommandValidator()
    {
        RuleFor(x => x.WebhookId)
            .NotEmpty();

        RuleFor(x => x.Url)
            .MustBeValidWebhookUrl();

        RuleFor(x => x.Events)
            .NotEmpty()
            .WithMessage("At least one event type is required.");
    }
}
