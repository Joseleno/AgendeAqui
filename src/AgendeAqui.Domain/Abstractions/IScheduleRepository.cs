using AgendeAqui.Domain.Schedules;

namespace AgendeAqui.Domain.Abstractions;

public interface IScheduleRepository : IRepository<Schedule>
{
    Task<IReadOnlyList<Schedule>> GetByProfessionalAsync(Guid professionalId, CancellationToken ct = default);
}
