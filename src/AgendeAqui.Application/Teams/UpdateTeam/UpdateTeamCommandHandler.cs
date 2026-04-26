using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Teams;

namespace AgendeAqui.Application.Teams.UpdateTeam;

internal sealed class UpdateTeamCommandHandler(
    ITeamRepository teamRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateTeamCommand>
{
    public async ValueTask<Result<Mediator.Unit>> Handle(UpdateTeamCommand command, CancellationToken cancellationToken)
    {
        var team = await teamRepository.GetByIdAsync(command.TeamId, cancellationToken);
        if (team is null)
            return Result.Failure<Mediator.Unit>(TeamErrors.NotFound);

        var result = team.Update(command.Name, command.Description);
        if (result.IsFailure)
            return Result.Failure<Mediator.Unit>(result.Error);

        teamRepository.Update(team);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Mediator.Unit.Value);
    }
}
