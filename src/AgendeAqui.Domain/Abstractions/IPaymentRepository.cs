using AgendeAqui.Domain.Payments;

namespace AgendeAqui.Domain.Abstractions;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<IReadOnlyList<Payment>> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken ct = default);
}
