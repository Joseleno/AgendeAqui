using FluentValidation;

namespace AgendeAqui.Application.Webhooks.RegisterWebhook;

public sealed class RegisterWebhookCommandValidator : AbstractValidator<RegisterWebhookCommand>
{
    public RegisterWebhookCommandValidator()
    {
        RuleFor(x => x.Url)
            .MustBeValidWebhookUrl();

        RuleFor(x => x.Secret)
            .NotEmpty()
            .MinimumLength(WebhookValidationRules.MinSecretLength);

        RuleFor(x => x.Events)
            .NotEmpty();
    }
}
