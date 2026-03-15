using FluentValidation;

namespace AgendeAqui.Application.ClinicalNotes.CreateClinicalNote;

public sealed class CreateClinicalNoteCommandValidator : AbstractValidator<CreateClinicalNoteCommand>
{
    public CreateClinicalNoteCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(10_000);
    }
}
