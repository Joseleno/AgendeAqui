using FluentValidation;

namespace AgendeAqui.Application.Absences.ListAbsences;

public sealed class ListAbsencesQueryValidator : AbstractValidator<ListAbsencesQuery>
{
    public ListAbsencesQueryValidator()
    {
        RuleFor(x => x.ProfessionalId).NotEmpty();
        RuleFor(x => x.From)
            .LessThanOrEqualTo(x => x.To)
            .WithMessage("'From' must be less than or equal to 'To'.");
    }
}
