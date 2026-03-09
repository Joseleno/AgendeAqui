using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Tenants;

namespace AgendeAqui.Application.Tenants.UpdateTenant;

public sealed class UpdateTenantCommandHandler(
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateTenantCommand>
{
    public async ValueTask<Result<Mediator.Unit>> Handle(
        UpdateTenantCommand command,
        CancellationToken cancellationToken)
    {
        var tenant = await tenantRepository.GetByIdAsync(command.TenantId, cancellationToken);

        if (tenant is null)
            return Result.Failure<Mediator.Unit>(TenantErrors.NotFound);

        if (!Enum.TryParse<TenantPlan>(command.Plan, true, out var plan))
            return Result.Failure<Mediator.Unit>(TenantErrors.InvalidPlan);

        tenant.UpdateName(command.Name);
        tenant.ChangePlan(plan);

        tenantRepository.Update(tenant);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Mediator.Unit.Value);
    }
}
