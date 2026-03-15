namespace AgendeAqui.Application.ClinicalNotes.GetClinicalNote;

public sealed record ClinicalNoteResponse(
    Guid Id,
    Guid ProfessionalId,
    string ProfessionalName,
    Guid ClientId,
    Guid? AppointmentId,
    string Title,
    string Content,
    bool IsPrivate,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
