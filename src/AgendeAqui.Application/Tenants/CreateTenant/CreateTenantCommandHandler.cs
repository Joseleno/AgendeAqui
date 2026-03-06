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

        var plan = Enum.Parse<TenantPlan>(command.Plan, true);

        var tenant = Tenant.Create(command.Name, command.Slug, plan);

        await tenantRepository.AddAsync(tenant, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(tenant.Id);
    }
}

public static class TenantErrors
{
    public static readonly Error SlugAlreadyExists = new("Tenant.SlugAlreadyExists", "A tenant with this slug already exists.");
    public static readonly Error NotFound = new("Tenant.NotFound", "Tenant not found.");
}
