using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Tenants;

namespace AgendeAqui.Application.Tenants.ActivateTenant;

internal sealed class ActivateTenantCommandHandler(
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<ActivateTenantCommand>
{
    public async ValueTask<Result<Mediator.Unit>> Handle(ActivateTenantCommand command, CancellationToken cancellationToken)
    {
        var tenant = await tenantRepository.GetByIdAsync(command.TenantId, cancellationToken);

        if (tenant is null)
            return Result.Failure<Mediator.Unit>(TenantErrors.NotFound);

        tenant.Activate();
        tenantRepository.Update(tenant);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Mediator.Unit.Value);
    }
}
