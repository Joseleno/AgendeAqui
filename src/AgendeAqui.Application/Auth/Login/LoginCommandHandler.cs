using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Users;

namespace AgendeAqui.Application.Auth.Login;

internal sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IJwtTokenGenerator tokenGenerator)
    : ICommandHandler<LoginCommand, LoginResponse>
{
    public async ValueTask<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByEmailAsync(request.Email.ToLowerInvariant(), ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Result.Failure<LoginResponse>(UserErrors.InvalidCredentials);

        var accessToken = tokenGenerator.GenerateToken(user.Id, user.TenantId, user.Email, user.Role, user.ProfessionalId, user.ClientId);
        var refreshToken = tokenGenerator.GenerateRefreshToken();

        return Result.Success(new LoginResponse(accessToken, refreshToken, 60));
    }
}
