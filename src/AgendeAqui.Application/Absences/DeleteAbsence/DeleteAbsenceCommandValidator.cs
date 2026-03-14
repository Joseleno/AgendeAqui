using FluentValidation;

namespace AgendeAqui.Application.Absences.DeleteAbsence;

public sealed class DeleteAbsenceCommandValidator : AbstractValidator<DeleteAbsenceCommand>
{
    public DeleteAbsenceCommandValidator()
    {
        RuleFor(x => x.AbsenceId).NotEmpty();
    }
}
