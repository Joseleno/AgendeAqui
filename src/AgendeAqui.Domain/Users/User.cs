using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Users;

public sealed class User : TenantEntity
{
    private User() { }

    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string Role { get; private set; } = default!;

    public static Result<User> Create(Guid tenantId, string email, string passwordHash, string name, string role)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<User>(UserErrors.EmptyEmail);
        if (string.IsNullOrWhiteSpace(passwordHash))
            return Result.Failure<User>(UserErrors.EmptyPassword);
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<User>(UserErrors.EmptyName);

        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            Name = name,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };

        return Result.Success(user);
    }
}
