using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.ApiKeys;
using AgendeAqui.Domain.Common;

namespace AgendeAqui.Application.ApiKeys.RevokeApiKey;

internal sealed class RevokeApiKeyCommandHandler(
    IApiKeyRepository apiKeyRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RevokeApiKeyCommand>
{
    public async ValueTask<Result<Mediator.Unit>> Handle(RevokeApiKeyCommand request, CancellationToken ct)
    {
        var apiKey = await apiKeyRepository.GetByIdAsync(request.ApiKeyId, ct);

        if (apiKey is null)
            return Result.Failure<Mediator.Unit>(ApiKeyErrors.NotFound);

        apiKey.Revoke();
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(Mediator.Unit.Value);
    }
}
