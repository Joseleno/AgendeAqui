using FluentValidation;

namespace AgendeAqui.Application.Absences.CreateAbsence;

public sealed class CreateAbsenceCommandValidator : AbstractValidator<CreateAbsenceCommand>
{
    public CreateAbsenceCommandValidator()
    {
        RuleFor(x => x.ProfessionalId).NotEmpty();
        RuleFor(x => x.Date).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
