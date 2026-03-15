using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.ClinicalNotes;

public static class ClinicalNoteErrors
{
    public static readonly Error NotFound = new("ClinicalNote.NotFound", "Clinical note not found.");
    public static readonly Error NotAuthorized = new("ClinicalNote.NotAuthorized", "You are not authorized to manage this clinical note.");
    public static readonly Error ClientNotFound = new("ClinicalNote.ClientNotFound", "Client not found.");
}
