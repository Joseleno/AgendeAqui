using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.ClinicalNotes.UpdateClinicalNote;

public sealed record UpdateClinicalNoteCommand(
    Guid Id,
    string Title,
    string Content,
    bool IsPrivate) : ICommand;
