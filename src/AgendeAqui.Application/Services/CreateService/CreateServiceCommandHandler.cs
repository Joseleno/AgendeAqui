using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Services;

namespace AgendeAqui.Application.Services.CreateService;

public sealed class CreateServiceCommandHandler(
    IServiceRepository serviceRepository,
    IUnitOfWork unitOfWork,
    ITenantProvider tenantProvider) : ICommandHandler<CreateServiceCommand, Guid>
{
    public async ValueTask<Result<Guid>> Handle(
        CreateServiceCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.GetTenantId();
        var duration = TimeSpan.FromMinutes(command.DurationMinutes);

        var result = Service.Create(tenantId, command.Name, duration, command.Price);

        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);

        var service = result.Value;

        await serviceRepository.AddAsync(service, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(service.Id);
    }
}
