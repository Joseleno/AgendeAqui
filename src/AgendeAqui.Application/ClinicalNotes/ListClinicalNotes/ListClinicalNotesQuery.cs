using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.ClinicalNotes.GetClinicalNote;
using AgendeAqui.Application.Common;

namespace AgendeAqui.Application.ClinicalNotes.ListClinicalNotes;

public sealed record ListClinicalNotesQuery : PagedRequest, IQuery<PagedResponse<ClinicalNoteResponse>>
{
    public Guid ClientId { get; init; }

    public ListClinicalNotesQuery(Guid clientId, int page = 1, int pageSize = 20)
    {
        ClientId = clientId;
        Page = page;
        PageSize = pageSize;
    }
}
