using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.ClinicalNotes.GetClinicalNote;

public sealed record GetClinicalNoteQuery(Guid Id) : IQuery<ClinicalNoteResponse>;
