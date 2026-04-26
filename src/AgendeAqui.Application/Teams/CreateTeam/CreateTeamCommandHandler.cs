using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Teams;

namespace AgendeAqui.Application.Teams.CreateTeam;

internal sealed class CreateTeamCommandHandler(
    ITeamRepository teamRepository,
    ITenantProvider tenantProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateTeamCommand, Guid>
{
    public async ValueTask<Result<Guid>> Handle(CreateTeamCommand command, CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.GetTenantId();

        var result = Team.Create(tenantId, command.Name, command.Description);
        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);

        await teamRepository.AddAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(result.Value.Id);
    }
}
