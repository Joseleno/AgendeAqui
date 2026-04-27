using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Tenants;

namespace AgendeAqui.Application.Tenants.SetCustomDomain;

internal sealed class SetCustomDomainCommandHandler(
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<SetCustomDomainCommand>
{
    public async ValueTask<Result<Mediator.Unit>> Handle(SetCustomDomainCommand command, CancellationToken cancellationToken)
    {
        var tenant = await tenantRepository.GetByIdAsync(command.TenantId, cancellationToken);
        if (tenant is null)
            return Result.Failure<Mediator.Unit>(TenantErrors.NotFound);

        if (command.Domain is not null)
        {
            var existing = await tenantRepository.GetByCustomDomainAsync(command.Domain.ToLowerInvariant(), cancellationToken);
            if (existing is not null && existing.Id != command.TenantId)
                return Result.Failure<Mediator.Unit>(TenantErrors.CustomDomainAlreadyTaken);
        }

        tenant.SetCustomDomain(command.Domain);
        tenantRepository.Update(tenant);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Mediator.Unit.Value);
    }
}
