using AgendeAqui.Domain.Professionals;

namespace AgendeAqui.Domain.Abstractions;

public interface IProfessionalServiceRepository
{
    Task<bool> ExistsAsync(Guid professionalId, Guid serviceId, CancellationToken ct = default);
    Task<ProfessionalService?> GetAsync(Guid professionalId, Guid serviceId, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> GetServiceIdsByProfessionalAsync(Guid professionalId, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> GetProfessionalIdsByServiceAsync(Guid serviceId, CancellationToken ct = default);
    Task<bool> HasAnyLinksAsync(Guid professionalId, CancellationToken ct = default);
    Task AddAsync(ProfessionalService link, CancellationToken ct = default);
    void Remove(ProfessionalService link);
}
