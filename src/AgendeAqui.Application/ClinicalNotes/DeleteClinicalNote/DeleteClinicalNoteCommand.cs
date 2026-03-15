using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.ClinicalNotes.DeleteClinicalNote;

public sealed record DeleteClinicalNoteCommand(Guid Id) : ICommand;
