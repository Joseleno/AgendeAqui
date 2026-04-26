using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Professionals;
using AgendeAqui.Domain.Teams;

namespace AgendeAqui.Application.Teams.AddTeamMember;

internal sealed class AddTeamMemberCommandHandler(
    ITeamRepository teamRepository,
    IProfessionalRepository professionalRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<AddTeamMemberCommand>
{
    public async ValueTask<Result<Mediator.Unit>> Handle(AddTeamMemberCommand command, CancellationToken cancellationToken)
    {
        var team = await teamRepository.GetByIdWithMembersAsync(command.TeamId, cancellationToken);
        if (team is null)
            return Result.Failure<Mediator.Unit>(TeamErrors.NotFound);

        var professional = await professionalRepository.GetByIdAsync(command.ProfessionalId, cancellationToken);
        if (professional is null)
            return Result.Failure<Mediator.Unit>(ProfessionalErrors.NotFound);

        var result = team.AddMember(professional.Id, professional.TenantId);
        if (result.IsFailure)
            return Result.Failure<Mediator.Unit>(result.Error);

        teamRepository.Update(team);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Mediator.Unit.Value);
    }
}
