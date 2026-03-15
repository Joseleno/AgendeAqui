using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.ClinicalNotes;
using AgendeAqui.Domain.Common;

namespace AgendeAqui.Application.ClinicalNotes.UpdateClinicalNote;

public sealed class UpdateClinicalNoteCommandHandler(
    IClinicalNoteRepository clinicalNoteRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<UpdateClinicalNoteCommand>
{
    public async ValueTask<Result<Mediator.Unit>> Handle(
        UpdateClinicalNoteCommand command,
        CancellationToken cancellationToken)
    {
        var note = await clinicalNoteRepository.GetByIdAsync(command.Id, cancellationToken);
        if (note is null)
            return Result.Failure<Mediator.Unit>(ClinicalNoteErrors.NotFound);

        if (note.ProfessionalId != currentUser.ProfessionalId)
            return Result.Failure<Mediator.Unit>(ClinicalNoteErrors.NotAuthorized);

        note.Update(command.Title, command.Content, command.IsPrivate);

        clinicalNoteRepository.Update(note);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Mediator.Unit.Value);
    }
}
