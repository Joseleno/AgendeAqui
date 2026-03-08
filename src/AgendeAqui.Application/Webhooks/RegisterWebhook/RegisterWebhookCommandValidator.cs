using FluentValidation;

namespace AgendeAqui.Application.Webhooks.RegisterWebhook;

public sealed class RegisterWebhookCommandValidator : AbstractValidator<RegisterWebhookCommand>
{
    public RegisterWebhookCommandValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty()
            .MaximumLength(2048);

        RuleFor(x => x.Secret)
            .NotEmpty()
            .MinimumLength(32);

        RuleFor(x => x.Events)
            .NotEmpty();
    }
}
