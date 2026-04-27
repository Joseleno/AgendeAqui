using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Tenants;

namespace AgendeAqui.Application.Tenants.UpdateTenantTheme;

internal sealed class UpdateTenantThemeCommandHandler(
    ITenantRepository tenantRepository,
    ITenantProvider tenantProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateTenantThemeCommand>
{
    public async ValueTask<Result<Mediator.Unit>> Handle(UpdateTenantThemeCommand command, CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.GetTenantId();
        var tenant = await tenantRepository.GetByIdAsync(tenantId, cancellationToken);

        if (tenant is null)
            return Result.Failure<Mediator.Unit>(TenantErrors.NotFound);

        var theme = new TenantTheme
        {
            PrimaryColor = command.PrimaryColor,
            LogoUrl = command.LogoUrl,
            FaviconUrl = command.FaviconUrl
        };

        tenant.UpdateTheme(theme);
        tenantRepository.Update(tenant);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Mediator.Unit.Value);
    }
}
