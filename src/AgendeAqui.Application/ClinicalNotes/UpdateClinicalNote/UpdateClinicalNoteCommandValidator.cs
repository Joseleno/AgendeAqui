using FluentValidation;

namespace AgendeAqui.Application.ClinicalNotes.UpdateClinicalNote;

public sealed class UpdateClinicalNoteCommandValidator : AbstractValidator<UpdateClinicalNoteCommand>
{
    public UpdateClinicalNoteCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(10_000);
    }
}
