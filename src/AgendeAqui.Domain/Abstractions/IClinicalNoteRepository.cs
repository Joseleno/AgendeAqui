using AgendeAqui.Domain.ClinicalNotes;

namespace AgendeAqui.Domain.Abstractions;

public interface IClinicalNoteRepository : IRepository<ClinicalNote>
{
    Task<IReadOnlyList<ClinicalNote>> GetByClientIdAsync(Guid clientId, int page, int pageSize, CancellationToken ct = default);
}
