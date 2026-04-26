using FluentValidation;
using AgendeAqui.Domain.Teams;

namespace AgendeAqui.Application.Teams.UpdateTeam;

internal sealed class UpdateTeamCommandValidator : AbstractValidator<UpdateTeamCommand>
{
    public UpdateTeamCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Team.MaxNameLength);
        RuleFor(x => x.Description).MaximumLength(Team.MaxDescriptionLength).When(x => x.Description is not null);
    }
}
