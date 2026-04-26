using FluentValidation;
using AgendeAqui.Domain.Teams;

namespace AgendeAqui.Application.Teams.CreateTeam;

internal sealed class CreateTeamCommandValidator : AbstractValidator<CreateTeamCommand>
{
    public CreateTeamCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Team name is required.")
            .MaximumLength(Team.MaxNameLength);

        RuleFor(x => x.Description)
            .MaximumLength(Team.MaxDescriptionLength)
            .When(x => x.Description is not null);
    }
}
