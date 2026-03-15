using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.ClinicalNotes;
using AgendeAqui.Domain.Common;

namespace AgendeAqui.Application.ClinicalNotes.DeleteClinicalNote;

public sealed class DeleteClinicalNoteCommandHandler(
    IClinicalNoteRepository clinicalNoteRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<DeleteClinicalNoteCommand>
{
    public async ValueTask<Result<Mediator.Unit>> Handle(
        DeleteClinicalNoteCommand command,
        CancellationToken cancellationToken)
    {
        var note = await clinicalNoteRepository.GetByIdAsync(command.Id, cancellationToken);
        if (note is null)
            return Result.Failure<Mediator.Unit>(ClinicalNoteErrors.NotFound);

        if (note.ProfessionalId != currentUser.ProfessionalId)
            return Result.Failure<Mediator.Unit>(ClinicalNoteErrors.NotAuthorized);

        clinicalNoteRepository.Remove(note);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Mediator.Unit.Value);
    }
}
