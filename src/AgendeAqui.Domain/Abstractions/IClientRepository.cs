using AgendeAqui.Domain.Clients;
using AgendeAqui.Domain.ValueObjects;

namespace AgendeAqui.Domain.Abstractions;

public interface IClientRepository : IRepository<Client>
{
    Task<Client?> GetByPhoneAsync(PhoneNumber phone, CancellationToken ct = default);
    Task<Client?> GetByEmailAsync(Email email, CancellationToken ct = default);
}
