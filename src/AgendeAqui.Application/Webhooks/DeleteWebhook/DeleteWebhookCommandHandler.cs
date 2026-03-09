using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Webhooks;

namespace AgendeAqui.Application.Webhooks.DeleteWebhook;

public sealed class DeleteWebhookCommandHandler(
    IWebhookRepository webhookRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteWebhookCommand>
{
    public async ValueTask<Result<Mediator.Unit>> Handle(DeleteWebhookCommand command, CancellationToken cancellationToken)
    {
        var webhook = await webhookRepository.GetByIdAsync(command.WebhookId, cancellationToken);

        if (webhook is null)
            return Result.Failure<Mediator.Unit>(WebhookErrors.NotFound);

        webhook.Delete();
        webhookRepository.Update(webhook);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Mediator.Unit.Value);
    }
}
