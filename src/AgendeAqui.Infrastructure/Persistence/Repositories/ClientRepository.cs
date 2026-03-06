using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Clients;
using AgendeAqui.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace AgendeAqui.Infrastructure.Persistence.Repositories;

internal sealed class ClientRepository : IClientRepository
{
    private readonly ApplicationDbContext _context;

    public ClientRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Client?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Clients.FindAsync([id], ct);

    public async Task<IReadOnlyList<Client>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Clients.AsNoTracking().ToListAsync(ct);

    public async Task<Client?> GetByPhoneAsync(PhoneNumber phone, CancellationToken ct = default) =>
        await _context.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Phone == phone, ct);

    public async Task<Client?> GetByEmailAsync(Email email, CancellationToken ct = default) =>
        await _context.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Email == email, ct);

    public async Task AddAsync(Client entity, CancellationToken ct = default) =>
        await _context.Clients.AddAsync(entity, ct);

    public void Update(Client entity) =>
        _context.Clients.Update(entity);

    public void Remove(Client entity) =>
        _context.Clients.Remove(entity);
}
