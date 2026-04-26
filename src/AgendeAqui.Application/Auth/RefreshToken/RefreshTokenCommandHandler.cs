using System.Security.Cryptography;
using System.Text;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Auth.Login;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Users;

namespace AgendeAqui.Application.Auth.RefreshToken;

internal sealed class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    IJwtTokenGenerator tokenGenerator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RefreshTokenCommand, LoginResponse>
{
    public async ValueTask<Result<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var tokenHash = ComputeHash(request.RefreshToken);

        var user = await userRepository.GetByRefreshTokenHashAsync(tokenHash, ct);

        if (user is null || !user.IsRefreshTokenValid(tokenHash))
            return Result.Failure<LoginResponse>(UserErrors.InvalidRefreshToken);

        var newAccessToken = tokenGenerator.GenerateToken(
            user.Id, user.TenantId, user.Email, user.Role, user.ProfessionalId, user.ClientId);

        var newRefreshToken = tokenGenerator.GenerateRefreshToken();
        var newRefreshTokenHash = ComputeHash(newRefreshToken);
        var expiresAt = DateTime.UtcNow.AddDays(30);

        user.SetRefreshToken(newRefreshTokenHash, expiresAt);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new LoginResponse(newAccessToken, newRefreshToken, tokenGenerator.ExpirationMinutes));
    }

    private static string ComputeHash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexStringLower(bytes);
    }
}
