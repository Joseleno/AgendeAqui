using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Users;

public sealed class User : TenantEntity
{
    private User() { }

    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string Role { get; private set; } = default!;
    public Guid? ProfessionalId { get; private set; }
    public Guid? ClientId { get; private set; }

    public string? RefreshTokenHash { get; private set; }
    public DateTime? RefreshTokenExpiresAt { get; private set; }

    public void SetRefreshToken(string tokenHash, DateTime expiresAt)
    {
        RefreshTokenHash = tokenHash;
        RefreshTokenExpiresAt = expiresAt;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsRefreshTokenValid(string tokenHash) =>
        RefreshTokenHash == tokenHash && RefreshTokenExpiresAt > DateTime.UtcNow;

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

    public Result LinkToProfessional(Guid professionalId)
    {
        if (ProfessionalId is not null)
            return Result.Failure(UserErrors.AlreadyLinkedToProfessional);

        ProfessionalId = professionalId;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result LinkToClient(Guid clientId)
    {
        if (ClientId is not null)
            return Result.Failure(UserErrors.AlreadyLinkedToClient);

        ClientId = clientId;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
