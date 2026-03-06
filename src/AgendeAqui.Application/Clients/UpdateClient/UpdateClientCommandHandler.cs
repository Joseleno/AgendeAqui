using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Clients;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.ValueObjects;

namespace AgendeAqui.Application.Clients.UpdateClient;

public sealed class UpdateClientCommandHandler(
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateClientCommand>
{
    public async ValueTask<Result<Mediator.Unit>> Handle(
        UpdateClientCommand command,
        CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdAsync(command.ClientId, cancellationToken);
        if (client is null)
            return Result.Failure<Mediator.Unit>(ClientErrors.NotFound);

        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
            return Result.Failure<Mediator.Unit>(emailResult.Error);

        var phoneResult = PhoneNumber.Create(command.Phone);
        if (phoneResult.IsFailure)
            return Result.Failure<Mediator.Unit>(phoneResult.Error);

        client.UpdateContact(command.Name, emailResult.Value, phoneResult.Value);

        clientRepository.Update(client);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Mediator.Unit.Value);
    }
}
