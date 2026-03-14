namespace AgendeAqui.Application.Auth;

public interface IJwtTokenGenerator
{
    int ExpirationMinutes { get; }
    string GenerateToken(Guid userId, Guid tenantId, string email, string role, Guid? professionalId = null, Guid? clientId = null);
    string GenerateRefreshToken();
}
