using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments;
using AgendeAqui.Domain.ClinicalNotes;
using AgendeAqui.Domain.Common;

namespace AgendeAqui.Application.ClinicalNotes.CreateClinicalNote;

public sealed class CreateClinicalNoteCommandHandler(
    IClinicalNoteRepository clinicalNoteRepository,
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork,
    ITenantProvider tenantProvider,
    ICurrentUser currentUser) : ICommandHandler<CreateClinicalNoteCommand, Guid>
{
    public async ValueTask<Result<Guid>> Handle(
        CreateClinicalNoteCommand command,
        CancellationToken cancellationToken)
    {
        if (currentUser.ProfessionalId is null)
            return Result.Failure<Guid>(ClinicalNoteErrors.NotAuthorized);

        var client = await clientRepository.GetByIdAsync(command.ClientId, cancellationToken);
        if (client is null)
            return Result.Failure<Guid>(AppointmentErrors.ClientNotFound);

        var tenantId = tenantProvider.GetTenantId();

        var clinicalNote = ClinicalNote.Create(
            tenantId,
            currentUser.ProfessionalId.Value,
            command.ClientId,
            command.AppointmentId,
            command.Title,
            command.Content,
            command.IsPrivate);

        await clinicalNoteRepository.AddAsync(clinicalNote, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(clinicalNote.Id);
    }
}
