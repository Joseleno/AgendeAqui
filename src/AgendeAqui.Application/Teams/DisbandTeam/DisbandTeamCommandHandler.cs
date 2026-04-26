using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Teams;

namespace AgendeAqui.Application.Teams.DisbandTeam;

internal sealed class DisbandTeamCommandHandler(
    ITeamRepository teamRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DisbandTeamCommand>
{
    public async ValueTask<Result<Mediator.Unit>> Handle(DisbandTeamCommand command, CancellationToken cancellationToken)
    {
        var team = await teamRepository.GetByIdAsync(command.TeamId, cancellationToken);
        if (team is null)
            return Result.Failure<Mediator.Unit>(TeamErrors.NotFound);

        team.Disband();
        teamRepository.Update(team);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Mediator.Unit.Value);
    }
}
