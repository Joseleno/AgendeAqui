using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.ValueObjects;

namespace AgendeAqui.Domain.Clients;

public sealed class Client : TenantEntity
{
    public string Name { get; private set; } = default!;
    public Email Email { get; private set; } = default!;
    public PhoneNumber Phone { get; private set; } = default!;

    private Client() { }

    public static Result<Client> Create(Guid tenantId, string name, Email email, PhoneNumber phone)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Client>(ClientErrors.InvalidName);

        var client = new Client
        {
            TenantId = tenantId,
            Name = name.Trim(),
            Email = email,
            Phone = phone
        };

        return Result.Success(client);
    }

    public Result UpdateContact(string name, Email email, PhoneNumber phone)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(ClientErrors.InvalidName);

        Name = name.Trim();
        Email = email;
        Phone = phone;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }
}
