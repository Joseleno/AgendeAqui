using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Tenants;

namespace AgendeAqui.Application.Tenants.CreateTenant;

public sealed class CreateTenantCommandHandler(
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateTenantCommand, Guid>
{
    public async ValueTask<Result<Guid>> Handle(
        CreateTenantCommand command,
        CancellationToken cancellationToken)
    {
        var existingTenant = await tenantRepository.GetBySlugAsync(command.Slug, cancellationToken);

        if (existingTenant is not null)
            return Result.Failure<Guid>(TenantErrors.SlugAlreadyExists);

        if (!Enum.TryParse<TenantPlan>(command.Plan, true, out var plan))
            return Result.Failure<Guid>(TenantErrors.InvalidPlan);

        var tenant = Tenant.Create(command.Name, command.Slug, plan);

        await tenantRepository.AddAsync(tenant, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(tenant.Id);
    }
}
