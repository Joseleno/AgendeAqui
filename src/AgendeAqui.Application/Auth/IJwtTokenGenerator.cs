namespace AgendeAqui.Application.Auth;

public interface IJwtTokenGenerator
{
    string GenerateToken(Guid userId, Guid tenantId, string email, string role);
    string GenerateRefreshToken();
}
