using FluentValidation;

namespace AgendeAqui.Application.ApiKeys.RevokeApiKey;

public sealed class RevokeApiKeyCommandValidator : AbstractValidator<RevokeApiKeyCommand>
{
    public RevokeApiKeyCommandValidator()
    {
        RuleFor(x => x.ApiKeyId).NotEmpty();
    }
}
