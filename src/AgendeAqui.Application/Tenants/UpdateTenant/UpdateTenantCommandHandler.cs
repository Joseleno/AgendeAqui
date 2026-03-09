using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Tenants.CreateTenant;
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

        var plan = Enum.Parse<TenantPlan>(command.Plan, true);

        tenant.UpdateName(command.Name);
        tenant.ChangePlan(plan);

        tenantRepository.Update(tenant);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Mediator.Unit.Value);
    }
}
