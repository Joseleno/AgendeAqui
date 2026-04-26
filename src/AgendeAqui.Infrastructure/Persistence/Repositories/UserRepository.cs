using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace AgendeAqui.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(ApplicationDbContext dbContext) : IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await dbContext.Set<User>().IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<User?> GetByProfessionalIdAsync(Guid professionalId, CancellationToken ct = default)
        => await dbContext.Set<User>().IgnoreQueryFilters().FirstOrDefaultAsync(u => u.ProfessionalId == professionalId, ct);

    public async Task<User?> GetByClientIdAsync(Guid clientId, CancellationToken ct = default)
        => await dbContext.Set<User>().IgnoreQueryFilters().FirstOrDefaultAsync(u => u.ClientId == clientId, ct);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        => await dbContext.Set<User>().IgnoreQueryFilters().AnyAsync(u => u.Email == email, ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
        => await dbContext.Set<User>().AddAsync(user, ct);

    public async Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken ct = default)
        => await dbContext.Set<User>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.RefreshTokenHash == tokenHash, ct);
}
