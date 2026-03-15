namespace AgendeAqui.Api.Endpoints.Requests;

public sealed record CreateClinicalNoteRequest(
    Guid ClientId,
    Guid? AppointmentId,
    string Title,
    string Content,
    bool IsPrivate);

public sealed record UpdateClinicalNoteRequest(
    string Title,
    string Content,
    bool IsPrivate);
