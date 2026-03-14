using AgendeAqui.Application.Auth;
using AgendeAqui.Application.Auth.Register;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Users;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Auth;

public class RegisterPatientCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly RegisterPatientCommandHandler _handler;

    private readonly Guid _tenantId = Guid.NewGuid();

    public RegisterPatientCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _clientRepository = Substitute.For<IClientRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _tokenGenerator = Substitute.For<IJwtTokenGenerator>();

        _tenantProvider.GetTenantId().Returns(_tenantId);

        _handler = new RegisterPatientCommandHandler(
            _userRepository,
            _clientRepository,
            _unitOfWork,
            _tenantProvider,
            _tokenGenerator);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateClientAndUserAndReturnToken()
    {
        // Arrange
        var command = new RegisterPatientCommand(
            "Maria Silva",
            "maria@example.com",
            "5511999887766",
            "SecurePass123!");

        _userRepository.ExistsByEmailAsync("maria@example.com", Arg.Any<CancellationToken>())
            .Returns(false);

        _tokenGenerator.GenerateToken(
                Arg.Any<Guid>(), _tenantId, "maria@example.com", "Client", null, Arg.Any<Guid>())
            .Returns("test-access-token");
        _tokenGenerator.GenerateRefreshToken().Returns("test-refresh-token");
        _tokenGenerator.ExpirationMinutes.Returns(60);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("test-access-token");
        result.Value.RefreshToken.Should().Be("test-refresh-token");
        result.Value.ExpiresInMinutes.Should().Be(60);

        await _clientRepository.Received(1).AddAsync(Arg.Any<Domain.Clients.Client>(), Arg.Any<CancellationToken>());
        await _userRepository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterPatientCommand(
            "Maria Silva",
            "existing@example.com",
            "5511999887766",
            "SecurePass123!");

        _userRepository.ExistsByEmailAsync("existing@example.com", Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.EmailAlreadyExists);

        await _clientRepository.DidNotReceive().AddAsync(Arg.Any<Domain.Clients.Client>(), Arg.Any<CancellationToken>());
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterPatientCommand(
            "Maria Silva",
            "not-an-email",
            "5511999887766",
            "SecurePass123!");

        _userRepository.ExistsByEmailAsync("not-an-email", Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        await _clientRepository.DidNotReceive().AddAsync(Arg.Any<Domain.Clients.Client>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
