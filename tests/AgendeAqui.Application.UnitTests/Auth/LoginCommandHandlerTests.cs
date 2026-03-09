using AgendeAqui.Application.Auth;
using AgendeAqui.Application.Auth.Login;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Users;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Auth;

public class LoginCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _tokenGenerator = Substitute.For<IJwtTokenGenerator>();
        _handler = new LoginCommandHandler(_userRepository, _tokenGenerator);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsToken()
    {
        var email = "user@example.com";
        var password = "SecurePass123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var tenantId = Guid.NewGuid();
        var userResult = User.Create(tenantId, email, passwordHash, "Test User", "Admin");
        var user = userResult.Value;

        _userRepository.GetByEmailAsync(email, Arg.Any<CancellationToken>()).Returns(user);
        _tokenGenerator.GenerateToken(user.Id, tenantId, email, "Admin").Returns("test-access-token");
        _tokenGenerator.GenerateRefreshToken().Returns("test-refresh-token");

        var command = new LoginCommand(email, password);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("test-access-token");
        result.Value.RefreshToken.Should().Be("test-refresh-token");
        result.Value.ExpiresInMinutes.Should().Be(60);
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ReturnsInvalidCredentials()
    {
        _userRepository.GetByEmailAsync("wrong@example.com", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var command = new LoginCommand("wrong@example.com", "anypassword");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidCredentials);
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ReturnsInvalidCredentials()
    {
        var email = "user@example.com";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("correct-password");
        var tenantId = Guid.NewGuid();
        var userResult = User.Create(tenantId, email, passwordHash, "Test User", "Admin");

        _userRepository.GetByEmailAsync(email, Arg.Any<CancellationToken>()).Returns(userResult.Value);

        var command = new LoginCommand(email, "wrong-password");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidCredentials);
        _tokenGenerator.DidNotReceive().GenerateToken(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>());
    }
}
