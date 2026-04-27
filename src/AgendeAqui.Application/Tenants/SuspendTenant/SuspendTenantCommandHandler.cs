using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Tenants;

namespace AgendeAqui.Application.Tenants.SuspendTenant;

internal sealed class SuspendTenantCommandHandler(
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<SuspendTenantCommand>
{
    public async ValueTask<Result<Mediator.Unit>> Handle(SuspendTenantCommand command, CancellationToken cancellationToken)
    {
        var tenant = await tenantRepository.GetByIdAsync(command.TenantId, cancellationToken);

        if (tenant is null)
            return Result.Failure<Mediator.Unit>(TenantErrors.NotFound);

        if (tenant.Status == TenantStatus.Suspended)
            return Result.Failure<Mediator.Unit>(TenantErrors.AlreadySuspended);

        tenant.Suspend();
        tenantRepository.Update(tenant);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Mediator.Unit.Value);
    }
}
