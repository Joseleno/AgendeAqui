using AgendeAqui.Application.Clients.CreateClient;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Clients;
using AgendeAqui.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Clients;

public class CreateClientCommandHandlerTests
{
    private readonly IClientRepository _clientRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly CreateClientCommandHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public CreateClientCommandHandlerTests()
    {
        _clientRepository = Substitute.For<IClientRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _tenantProvider.GetTenantId().Returns(_tenantId);
        _handler = new CreateClientCommandHandler(_clientRepository, _unitOfWork, _tenantProvider);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateClientAndReturnId()
    {
        var command = new CreateClientCommand("Maria Silva", "maria@example.com", "5511987654321");
        _clientRepository.GetByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns((Client?)null);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _clientRepository.Received(1).AddAsync(Arg.Any<Client>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldReturnFailure()
    {
        var command = new CreateClientCommand("Maria Silva", "invalid", "5511987654321");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Email.InvalidEmail);
        await _clientRepository.DidNotReceive().AddAsync(Arg.Any<Client>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidPhone_ShouldReturnFailure()
    {
        var command = new CreateClientCommand("Maria Silva", "maria@example.com", "invalid");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PhoneNumber.InvalidPhone);
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ShouldReturnFailure()
    {
        var command = new CreateClientCommand("Maria Silva", "maria@example.com", "5511987654321");
        var existingClient = Client.Create(Guid.NewGuid(), "Existing", Email.Create("maria@example.com").Value, PhoneNumber.Create("5521912345678").Value).Value;
        _clientRepository.GetByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns(existingClient);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ClientErrors.EmailAlreadyExists);
        await _clientRepository.DidNotReceive().AddAsync(Arg.Any<Client>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptyName_ShouldReturnFailure()
    {
        var command = new CreateClientCommand("", "maria@example.com", "5511987654321");
        _clientRepository.GetByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns((Client?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ClientErrors.InvalidName);
    }
}
