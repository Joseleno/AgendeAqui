using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.ClinicalNotes.CreateClinicalNote;

public sealed record CreateClinicalNoteCommand(
    Guid ClientId,
    Guid? AppointmentId,
    string Title,
    string Content,
    bool IsPrivate) : ICommand<Guid>;
