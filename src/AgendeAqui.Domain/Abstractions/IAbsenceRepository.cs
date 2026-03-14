using AgendeAqui.Domain.Schedules;

namespace AgendeAqui.Domain.Abstractions;

public interface IAbsenceRepository : IRepository<Absence>
{
    Task<IReadOnlyList<Absence>> GetByProfessionalAndDateRangeAsync(
        Guid professionalId,
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default);

    Task<IReadOnlyList<Absence>> GetByProfessionalAndDateAsync(
        Guid professionalId,
        DateOnly date,
        CancellationToken ct = default);
}
