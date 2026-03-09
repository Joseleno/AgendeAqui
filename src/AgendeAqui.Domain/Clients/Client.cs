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
        if (tenantId == Guid.Empty)
            return Result.Failure<Client>(ClientErrors.InvalidTenant);

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

    private const string AnonymizedName = "***ANONYMIZED***";

    // Anonymized placeholder values — must match PhoneNumber/Email validation rules.
    // "5500000000000" is E.164 compliant (13 digits, stored as +5500000000000).
    private static readonly Email AnonymizedEmail = Email.Create("anonymized@removed.com").Value;
    private static readonly PhoneNumber AnonymizedPhone = PhoneNumber.Create("5500000000000").Value;

    public bool IsAnonymized => Name == AnonymizedName;

    public void Anonymize()
    {
        Name = AnonymizedName;
        Email = AnonymizedEmail;
        Phone = AnonymizedPhone;
        UpdatedAt = DateTime.UtcNow;
    }
}
