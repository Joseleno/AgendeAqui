using FluentValidation;

namespace AgendeAqui.Application.ClinicalNotes.DeleteClinicalNote;

public sealed class DeleteClinicalNoteCommandValidator : AbstractValidator<DeleteClinicalNoteCommand>
{
    public DeleteClinicalNoteCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
