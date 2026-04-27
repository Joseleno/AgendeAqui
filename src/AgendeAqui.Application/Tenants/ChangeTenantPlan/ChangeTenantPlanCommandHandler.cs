using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Tenants;

namespace AgendeAqui.Application.Tenants.ChangeTenantPlan;

internal sealed class ChangeTenantPlanCommandHandler(
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<ChangeTenantPlanCommand>
{
    public async ValueTask<Result<Mediator.Unit>> Handle(ChangeTenantPlanCommand command, CancellationToken cancellationToken)
    {
        var tenant = await tenantRepository.GetByIdAsync(command.TenantId, cancellationToken);

        if (tenant is null)
            return Result.Failure<Mediator.Unit>(TenantErrors.NotFound);

        if (!Enum.TryParse<TenantPlan>(command.Plan, true, out var plan))
            return Result.Failure<Mediator.Unit>(TenantErrors.InvalidPlan);

        tenant.ChangePlan(plan);
        tenantRepository.Update(tenant);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Mediator.Unit.Value);
    }
}
