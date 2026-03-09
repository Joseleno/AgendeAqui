using System.Security.Cryptography;
using System.Text;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Webhooks;

namespace AgendeAqui.Application.Webhooks.RegisterWebhook;

public sealed class RegisterWebhookCommandHandler(
    IWebhookRepository webhookRepository,
    IUnitOfWork unitOfWork,
    ITenantProvider tenantProvider) : ICommandHandler<RegisterWebhookCommand, Guid>
{
    public async ValueTask<Result<Guid>> Handle(RegisterWebhookCommand command, CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.GetTenantId();

        var secretBytes = Encoding.UTF8.GetBytes(command.Secret);
        var hashBytes = SHA256.HashData(secretBytes);
        var secretHash = Convert.ToHexStringLower(hashBytes);

        var result = Webhook.Create(tenantId, command.Url, secretHash, command.Events);

        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);

        var webhook = result.Value;

        await webhookRepository.AddAsync(webhook, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(webhook.Id);
    }
}
