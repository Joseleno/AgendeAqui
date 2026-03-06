using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Clients;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.ValueObjects;

namespace AgendeAqui.Application.Clients.CreateClient;

public sealed class CreateClientCommandHandler(
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork,
    ITenantProvider tenantProvider) : ICommandHandler<CreateClientCommand, Guid>
{
    public async ValueTask<Result<Guid>> Handle(
        CreateClientCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.GetTenantId();

        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
            return Result.Failure<Guid>(emailResult.Error);

        var phoneResult = PhoneNumber.Create(command.Phone);
        if (phoneResult.IsFailure)
            return Result.Failure<Guid>(phoneResult.Error);

        var existingClient = await clientRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (existingClient is not null)
            return Result.Failure<Guid>(ClientErrors.EmailAlreadyExists);

        var clientResult = Client.Create(tenantId, command.Name, emailResult.Value, phoneResult.Value);
        if (clientResult.IsFailure)
            return Result.Failure<Guid>(clientResult.Error);

        var client = clientResult.Value;

        await clientRepository.AddAsync(client, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(client.Id);
    }
}
