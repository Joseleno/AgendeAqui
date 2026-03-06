using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.ValueObjects;

namespace AgendeAqui.Domain.Professionals;

public sealed class Professional : TenantEntity
{
    public string Name { get; private set; } = default!;
    public Email Email { get; private set; } = default!;
    public PhoneNumber Phone { get; private set; } = default!;
    public bool IsActive { get; private set; }

    private Professional() { }

    public static Result<Professional> Create(Guid tenantId, string name, Email email, PhoneNumber phone)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Professional>(ProfessionalErrors.InvalidName);

        var professional = new Professional
        {
            TenantId = tenantId,
            Name = name.Trim(),
            Email = email,
            Phone = phone,
            IsActive = true
        };

        return Result.Success(professional);
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateContact(string name, Email email, PhoneNumber phone)
    {
        Name = name.Trim();
        Email = email;
        Phone = phone;
        UpdatedAt = DateTime.UtcNow;
    }
}
