using AgendeAqui.Application.Auth;
using AgendeAqui.Application.Auth.Login;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Users;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Auth;

public class LoginWithLinkedEntitiesTests
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly LoginCommandHandler _handler;

    public LoginWithLinkedEntitiesTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _tokenGenerator = Substitute.For<IJwtTokenGenerator>();
        _handler = new LoginCommandHandler(_userRepository, _tokenGenerator);
    }

    [Fact]
    public async Task Handle_WithUserLinkedToProfessional_ShouldPassProfessionalIdToGenerateToken()
    {
        var email = "pro@example.com";
        var password = "SecurePass123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var tenantId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();

        var user = User.Create(tenantId, email, passwordHash, "Dr. John", "Professional").Value;
        user.LinkToProfessional(professionalId);

        _userRepository.GetByEmailAsync(email, Arg.Any<CancellationToken>()).Returns(user);
        _tokenGenerator.GenerateToken(user.Id, tenantId, email, "Professional", professionalId, null).Returns("pro-token");
        _tokenGenerator.GenerateRefreshToken().Returns("refresh-token");

        var command = new LoginCommand(email, password);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("pro-token");
        _tokenGenerator.Received(1).GenerateToken(user.Id, tenantId, email, "Professional", professionalId, null);
    }

    [Fact]
    public async Task Handle_WithUserLinkedToClient_ShouldPassClientIdToGenerateToken()
    {
        var email = "client@example.com";
        var password = "SecurePass123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var tenantId = Guid.NewGuid();
        var clientId = Guid.NewGuid();

        var user = User.Create(tenantId, email, passwordHash, "Maria", "Client").Value;
        user.LinkToClient(clientId);

        _userRepository.GetByEmailAsync(email, Arg.Any<CancellationToken>()).Returns(user);
        _tokenGenerator.GenerateToken(user.Id, tenantId, email, "Client", null, clientId).Returns("client-token");
        _tokenGenerator.GenerateRefreshToken().Returns("refresh-token");

        var command = new LoginCommand(email, password);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("client-token");
        _tokenGenerator.Received(1).GenerateToken(user.Id, tenantId, email, "Client", null, clientId);
    }

    [Fact]
    public async Task Handle_WithUnlinkedUser_ShouldPassNullForBothIds()
    {
        var email = "admin@example.com";
        var password = "SecurePass123!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var tenantId = Guid.NewGuid();

        var user = User.Create(tenantId, email, passwordHash, "Admin User", "Admin").Value;

        _userRepository.GetByEmailAsync(email, Arg.Any<CancellationToken>()).Returns(user);
        _tokenGenerator.GenerateToken(user.Id, tenantId, email, "Admin", null, null).Returns("admin-token");
        _tokenGenerator.GenerateRefreshToken().Returns("refresh-token");

        var command = new LoginCommand(email, password);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("admin-token");
        _tokenGenerator.Received(1).GenerateToken(user.Id, tenantId, email, "Admin", null, null);
    }
}
