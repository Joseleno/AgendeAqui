using AgendeAqui.Domain.Appointments;

namespace AgendeAqui.Domain.Abstractions;

public interface IAppointmentRepository : IRepository<Appointment>
{
    Task<IReadOnlyList<Appointment>> GetByProfessionalAndDateAsync(Guid professionalId, DateOnly date, CancellationToken ct = default);
    Task<bool> HasConflictAsync(Guid professionalId, DateOnly date, TimeOnly start, TimeOnly end, CancellationToken ct = default);
}
