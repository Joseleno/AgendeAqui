using AgendeAqui.Application.Clients.UpdateClient;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Clients;
using AgendeAqui.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Clients;

public class UpdateClientCommandHandlerTests
{
    private readonly IClientRepository _clientRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateClientCommandHandler _handler;

    public UpdateClientCommandHandlerTests()
    {
        _clientRepository = Substitute.For<IClientRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new UpdateClientCommandHandler(_clientRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateClient()
    {
        var client = Client.Create(
            Guid.NewGuid(), "Maria",
            Email.Create("maria@example.com").Value,
            PhoneNumber.Create("5511987654321").Value).Value;

        _clientRepository.GetByIdAsync(client.Id, Arg.Any<CancellationToken>())
            .Returns(client);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = new UpdateClientCommand(client.Id, "Maria Updated", "new@example.com", "5521912345678");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        client.Name.Should().Be("Maria Updated");
        _clientRepository.Received(1).Update(client);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentClient_ShouldReturnNotFound()
    {
        _clientRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Client?)null);

        var command = new UpdateClientCommand(Guid.NewGuid(), "Maria", "maria@example.com", "5521912345678");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ClientErrors.NotFound);
        _clientRepository.DidNotReceive().Update(Arg.Any<Client>());
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldReturnFailure()
    {
        var client = Client.Create(
            Guid.NewGuid(), "Maria",
            Email.Create("maria@example.com").Value,
            PhoneNumber.Create("5511987654321").Value).Value;

        _clientRepository.GetByIdAsync(client.Id, Arg.Any<CancellationToken>())
            .Returns(client);

        var command = new UpdateClientCommand(client.Id, "Maria", "invalid", "5521912345678");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Email.InvalidEmail);
    }

    [Fact]
    public async Task Handle_WithEmptyName_ShouldReturnFailure()
    {
        var client = Client.Create(
            Guid.NewGuid(), "Maria",
            Email.Create("maria@example.com").Value,
            PhoneNumber.Create("5511987654321").Value).Value;

        _clientRepository.GetByIdAsync(client.Id, Arg.Any<CancellationToken>())
            .Returns(client);

        var command = new UpdateClientCommand(client.Id, "", "maria@example.com", "5521912345678");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ClientErrors.InvalidName);
    }

    [Fact]
    public async Task Handle_WithInvalidPhone_ShouldReturnFailure()
    {
        var client = Client.Create(
            Guid.NewGuid(), "Maria",
            Email.Create("maria@example.com").Value,
            PhoneNumber.Create("5511987654321").Value).Value;

        _clientRepository.GetByIdAsync(client.Id, Arg.Any<CancellationToken>())
            .Returns(client);

        var command = new UpdateClientCommand(client.Id, "Maria", "maria@example.com", "invalid");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PhoneNumber.InvalidPhone);
    }
}
