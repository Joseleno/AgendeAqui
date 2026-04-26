using System.Security.Cryptography;
using System.Text;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Users;

namespace AgendeAqui.Application.Auth.Login;

internal sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IJwtTokenGenerator tokenGenerator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<LoginCommand, LoginResponse>
{
    public async ValueTask<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByEmailAsync(request.Email.ToLowerInvariant(), ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Result.Failure<LoginResponse>(UserErrors.InvalidCredentials);

        var accessToken = tokenGenerator.GenerateToken(user.Id, user.TenantId, user.Email, user.Role, user.ProfessionalId, user.ClientId);
        var refreshToken = tokenGenerator.GenerateRefreshToken();
        var refreshTokenHash = ComputeHash(refreshToken);

        user.SetRefreshToken(refreshTokenHash, DateTime.UtcNow.AddDays(30));
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new LoginResponse(accessToken, refreshToken, tokenGenerator.ExpirationMinutes));
    }

    private static string ComputeHash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexStringLower(bytes);
    }
}
