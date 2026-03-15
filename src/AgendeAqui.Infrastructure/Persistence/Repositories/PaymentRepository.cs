using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Payments;
using Microsoft.EntityFrameworkCore;

namespace AgendeAqui.Infrastructure.Persistence.Repositories;

internal sealed class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _context;

    public PaymentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Payments.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<Payment>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Payments.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(Payment entity, CancellationToken ct = default) =>
        await _context.Payments.AddAsync(entity, ct);

    public void Update(Payment entity) =>
        _context.Payments.Update(entity);

    public void Remove(Payment entity) =>
        _context.Payments.Remove(entity);

    public async Task<IReadOnlyList<Payment>> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken ct = default) =>
        await _context.Payments
            .Where(p => p.AppointmentId == appointmentId)
            .AsNoTracking()
            .ToListAsync(ct);
}
