using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Webhooks;

namespace AgendeAqui.Application.Webhooks.UpdateWebhook;

public sealed class UpdateWebhookCommandHandler(
    IWebhookRepository webhookRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateWebhookCommand>
{
    public async ValueTask<Result<Mediator.Unit>> Handle(
        UpdateWebhookCommand command,
        CancellationToken cancellationToken)
    {
        var webhook = await webhookRepository.GetByIdAsync(command.WebhookId, cancellationToken);

        if (webhook is null)
            return Result.Failure<Mediator.Unit>(WebhookErrors.NotFound);

        var result = webhook.Update(command.Url, command.Events, command.IsActive);
        if (result.IsFailure)
            return Result.Failure<Mediator.Unit>(result.Error);

        webhookRepository.Update(webhook);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Mediator.Unit.Value);
    }
}
