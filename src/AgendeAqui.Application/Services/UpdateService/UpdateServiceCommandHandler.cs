using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Services;

namespace AgendeAqui.Application.Services.UpdateService;

public sealed class UpdateServiceCommandHandler(
    IServiceRepository serviceRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateServiceCommand>
{
    public async ValueTask<Result<Mediator.Unit>> Handle(
        UpdateServiceCommand command,
        CancellationToken cancellationToken)
    {
        var service = await serviceRepository.GetByIdAsync(command.ServiceId, cancellationToken);

        if (service is null)
            return Result.Failure<Mediator.Unit>(ServiceErrors.NotFound);

        service.UpdateDetails(
            command.Name,
            TimeSpan.FromMinutes(command.DurationMinutes),
            command.Price);

        serviceRepository.Update(service);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Mediator.Unit.Value);
    }
}
