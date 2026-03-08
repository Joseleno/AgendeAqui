using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Clients;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.DataProtection;

namespace AgendeAqui.Application.Clients.DeleteClientData;

public sealed class DeleteClientDataCommandHandler(
    IClientRepository clientRepository,
    IDataDeletionRequestRepository dataDeletionRequestRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteClientDataCommand>
{
    public async ValueTask<Result<Mediator.Unit>> Handle(
        DeleteClientDataCommand command,
        CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdAsync(command.ClientId, cancellationToken);
        if (client is null)
            return Result.Failure<Mediator.Unit>(ClientErrors.NotFound);

        if (client.IsAnonymized)
            return Result.Failure<Mediator.Unit>(ClientErrors.AlreadyAnonymized);

        // Create the deletion request first (Status = Pending) as LGPD audit trail.
        var requestResult = DataDeletionRequest.Create(client.TenantId, client.Id);
        if (requestResult.IsFailure)
            return Result.Failure<Mediator.Unit>(requestResult.Error);

        var deletionRequest = requestResult.Value;
        await dataDeletionRequestRepository.AddAsync(deletionRequest, cancellationToken);

        // Anonymize the client data and mark the request as completed in the same transaction.
        client.Anonymize();
        clientRepository.Update(client);
        deletionRequest.Complete();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Mediator.Unit.Value);
    }
}
