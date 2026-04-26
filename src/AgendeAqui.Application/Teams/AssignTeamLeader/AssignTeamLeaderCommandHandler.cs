using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Teams;

namespace AgendeAqui.Application.Teams.AssignTeamLeader;

internal sealed class AssignTeamLeaderCommandHandler(
    ITeamRepository teamRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<AssignTeamLeaderCommand>
{
    public async ValueTask<Result<Mediator.Unit>> Handle(AssignTeamLeaderCommand command, CancellationToken cancellationToken)
    {
        var team = await teamRepository.GetByIdWithMembersAsync(command.TeamId, cancellationToken);
        if (team is null)
            return Result.Failure<Mediator.Unit>(TeamErrors.NotFound);

        var result = command.ProfessionalId.HasValue
            ? team.AssignLeader(command.ProfessionalId.Value)
            : team.ClearLeader();

        if (result.IsFailure)
            return Result.Failure<Mediator.Unit>(result.Error);

        teamRepository.Update(team);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Mediator.Unit.Value);
    }
}
