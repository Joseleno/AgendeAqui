using AgendeAqui.Domain.Schedules;

namespace AgendeAqui.Domain.Abstractions;

public interface IScheduleRepository : IRepository<Schedule>
{
    Task<IReadOnlyList<Schedule>> GetByProfessionalAsync(Guid professionalId, CancellationToken ct = default);
    Task<Schedule?> GetByProfessionalAndDayAsync(Guid professionalId, DayOfWeek dayOfWeek, CancellationToken ct = default);
}
