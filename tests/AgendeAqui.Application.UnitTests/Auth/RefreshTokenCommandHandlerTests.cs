using System.Security.Cryptography;
using System.Text;
using AgendeAqui.Application.Auth;
using AgendeAqui.Application.Auth.RefreshToken;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Users;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Auth;

public class RefreshTokenCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _tokenGenerator = Substitute.For<IJwtTokenGenerator>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new RefreshTokenCommandHandler(_userRepository, _tokenGenerator, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidRefreshToken_ReturnsNewTokenPair()
    {
        var tenantId = Guid.NewGuid();
        var rawToken = "valid-refresh-token";
        var tokenHash = ComputeHash(rawToken);

        var userResult = User.Create(tenantId, "user@test.com", "hash", "Test User", "Admin");
        var user = userResult.Value;
        user.SetRefreshToken(tokenHash, DateTime.UtcNow.AddDays(30));

        _userRepository.GetByRefreshTokenHashAsync(tokenHash, Arg.Any<CancellationToken>()).Returns(user);
        _tokenGenerator.GenerateToken(user.Id, tenantId, user.Email, user.Role, null, null).Returns("new-access-token");
        _tokenGenerator.GenerateRefreshToken().Returns("new-refresh-token");
        _tokenGenerator.ExpirationMinutes.Returns(60);

        var result = await _handler.Handle(new RefreshTokenCommand(rawToken), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("new-access-token");
        result.Value.RefreshToken.Should().Be("new-refresh-token");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnknownToken_ReturnsInvalidRefreshToken()
    {
        _userRepository.GetByRefreshTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var result = await _handler.Handle(new RefreshTokenCommand("unknown-token"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidRefreshToken);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithExpiredRefreshToken_ReturnsInvalidRefreshToken()
    {
        var tenantId = Guid.NewGuid();
        var rawToken = "expired-refresh-token";
        var tokenHash = ComputeHash(rawToken);

        var userResult = User.Create(tenantId, "user@test.com", "hash", "Test User", "Admin");
        var user = userResult.Value;
        user.SetRefreshToken(tokenHash, DateTime.UtcNow.AddDays(-1)); // expired

        _userRepository.GetByRefreshTokenHashAsync(tokenHash, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.Handle(new RefreshTokenCommand(rawToken), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidRefreshToken);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static string ComputeHash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexStringLower(bytes);
    }
}
