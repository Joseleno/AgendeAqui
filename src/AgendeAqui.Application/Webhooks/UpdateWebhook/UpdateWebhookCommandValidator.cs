using FluentValidation;

namespace AgendeAqui.Application.Webhooks.UpdateWebhook;

public sealed class UpdateWebhookCommandValidator : AbstractValidator<UpdateWebhookCommand>
{
    public UpdateWebhookCommandValidator()
    {
        RuleFor(x => x.WebhookId)
            .NotEmpty();

        RuleFor(x => x.Url)
            .NotEmpty()
            .MaximumLength(2048)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                         (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("URL must be a valid HTTP or HTTPS address.");

        RuleFor(x => x.Events)
            .NotEmpty()
            .WithMessage("At least one event type is required.");
    }
}
