using System.Security.Cryptography;
using System.Text;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.ApiKeys;
using AgendeAqui.Domain.Common;

namespace AgendeAqui.Application.ApiKeys.CreateApiKey;

internal sealed class CreateApiKeyCommandHandler(
    IApiKeyRepository apiKeyRepository,
    IUnitOfWork unitOfWork,
    ITenantProvider tenantProvider)
    : ICommandHandler<CreateApiKeyCommand, CreateApiKeyResponse>
{
    public async ValueTask<Result<CreateApiKeyResponse>> Handle(
        CreateApiKeyCommand request,
        CancellationToken ct)
    {
        var tenantId = tenantProvider.GetTenantId();

        var rawKey = GenerateRawKey();
        var keyHash = ComputeSha256Hash(rawKey);

        var apiKeyResult = ApiKey.Create(tenantId, request.Name, keyHash, request.ExpiresAt);
        if (apiKeyResult.IsFailure)
            return Result.Failure<CreateApiKeyResponse>(apiKeyResult.Error);

        var apiKey = apiKeyResult.Value;

        await apiKeyRepository.AddAsync(apiKey, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new CreateApiKeyResponse(apiKey.Id, rawKey, apiKey.Name, apiKey.ExpiresAt));
    }

    private static string GenerateRawKey()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return $"sk_live_{Convert.ToHexStringLower(randomBytes)}";
    }

    private static string ComputeSha256Hash(string rawKey)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawKey));
        return Convert.ToHexStringLower(hashBytes);
    }
}
