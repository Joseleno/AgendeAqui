using AgendeAqui.Domain.Users;

namespace AgendeAqui.Domain.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByProfessionalIdAsync(Guid professionalId, CancellationToken ct = default);
    Task<User?> GetByClientIdAsync(Guid clientId, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken ct = default);
}
