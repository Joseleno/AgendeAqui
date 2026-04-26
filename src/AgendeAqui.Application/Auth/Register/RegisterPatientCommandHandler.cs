using System.Security.Cryptography;
using System.Text;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Auth.Login;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Clients;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Users;
using AgendeAqui.Domain.ValueObjects;

namespace AgendeAqui.Application.Auth.Register;

internal sealed class RegisterPatientCommandHandler(
    IUserRepository userRepository,
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork,
    ITenantProvider tenantProvider,
    IJwtTokenGenerator tokenGenerator)
    : ICommandHandler<RegisterPatientCommand, LoginResponse>
{
    public async ValueTask<Result<LoginResponse>> Handle(RegisterPatientCommand request, CancellationToken ct)
    {
        var tenantId = tenantProvider.GetTenantId();

        var emailLower = request.Email.ToLowerInvariant();

        if (await userRepository.ExistsByEmailAsync(emailLower, ct))
            return Result.Failure<LoginResponse>(UserErrors.EmailAlreadyExists);

        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
            return Result.Failure<LoginResponse>(emailResult.Error);

        var phoneResult = PhoneNumber.Create(request.Phone);
        if (phoneResult.IsFailure)
            return Result.Failure<LoginResponse>(phoneResult.Error);

        var clientResult = Client.Create(tenantId, request.Name, emailResult.Value, phoneResult.Value);
        if (clientResult.IsFailure)
            return Result.Failure<LoginResponse>(clientResult.Error);

        var client = clientResult.Value;
        await clientRepository.AddAsync(client, ct);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var userResult = User.Create(tenantId, emailLower, passwordHash, request.Name, "Client");
        if (userResult.IsFailure)
            return Result.Failure<LoginResponse>(userResult.Error);

        var user = userResult.Value;
        user.LinkToClient(client.Id);

        await userRepository.AddAsync(user, ct);

        var accessToken = tokenGenerator.GenerateToken(
            user.Id, tenantId, user.Email, user.Role, null, client.Id);
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
