using FluentValidation;

namespace AgendeAqui.Application.Webhooks;

public static class WebhookValidationRules
{
    public const int MaxUrlLength = 2048;
    public const int MinSecretLength = 32;

    public static IRuleBuilderOptions<T, string> MustBeValidWebhookUrl<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .MaximumLength(MaxUrlLength)
            .Must(WebhookUrlValidator.IsValidHttpUrl)
            .WithMessage("Webhook URL must be a valid HTTP or HTTPS URL.")
            .MustAsync(async (url, ct) => !await WebhookUrlValidator.ResolvesToPrivateIpAsync(url, ct))
            .WithMessage("Webhook URL must not resolve to a private or loopback IP address.");
    }
}
