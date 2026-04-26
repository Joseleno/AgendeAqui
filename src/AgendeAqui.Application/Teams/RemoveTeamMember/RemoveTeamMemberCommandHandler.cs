using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Teams;

namespace AgendeAqui.Application.Teams.RemoveTeamMember;

internal sealed class RemoveTeamMemberCommandHandler(
    ITeamRepository teamRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<RemoveTeamMemberCommand>
{
    public async ValueTask<Result<Mediator.Unit>> Handle(RemoveTeamMemberCommand command, CancellationToken cancellationToken)
    {
        var team = await teamRepository.GetByIdWithMembersAsync(command.TeamId, cancellationToken);
        if (team is null)
            return Result.Failure<Mediator.Unit>(TeamErrors.NotFound);

        var result = team.RemoveMember(command.ProfessionalId);
        if (result.IsFailure)
            return Result.Failure<Mediator.Unit>(result.Error);

        teamRepository.Update(team);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Mediator.Unit.Value);
    }
}
