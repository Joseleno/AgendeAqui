using FluentValidation;

namespace AgendeAqui.Application.Teams.AddTeamMember;

internal sealed class AddTeamMemberCommandValidator : AbstractValidator<AddTeamMemberCommand>
{
    public AddTeamMemberCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.ProfessionalId).NotEmpty();
    }
}
