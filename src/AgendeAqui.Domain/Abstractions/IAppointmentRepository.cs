using AgendeAqui.Domain.Appointments;

namespace AgendeAqui.Domain.Abstractions;

public interface IAppointmentRepository : IRepository<Appointment>
{
    Task<IReadOnlyList<Appointment>> GetByProfessionalAndDateAsync(Guid professionalId, DateOnly date, CancellationToken ct = default);
    Task<bool> HasConflictAsync(Guid professionalId, DateOnly date, TimeOnly start, TimeOnly end, Guid? excludeAppointmentId = null, CancellationToken ct = default);
    Task<IReadOnlyList<Appointment>> GetByDateRangeAsync(Guid professionalId, DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<List<Appointment>> GetByDateRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
}
