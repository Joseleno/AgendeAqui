using AgendeAqui.Application.Clients.DeleteClientData;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Clients;
using AgendeAqui.Domain.DataProtection;
using AgendeAqui.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Clients;

public class DeleteClientDataCommandHandlerTests
{
    private readonly IClientRepository _clientRepository;
    private readonly IDataDeletionRequestRepository _dataDeletionRequestRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeleteClientDataCommandHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public DeleteClientDataCommandHandlerTests()
    {
        _clientRepository = Substitute.For<IClientRepository>();
        _dataDeletionRequestRepository = Substitute.For<IDataDeletionRequestRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new DeleteClientDataCommandHandler(_clientRepository, _dataDeletionRequestRepository, _unitOfWork);
    }

    private Client CreateClient() =>
        Client.Create(
            _tenantId,
            "Maria Silva",
            Email.Create("maria@test.com").Value,
            PhoneNumber.Create("5521912345678").Value).Value;

    [Fact]
    public async Task Handle_ExistingClient_ShouldAnonymizeAndCreateDeletionRequest()
    {
        // Arrange
        var client = CreateClient();
        var command = new DeleteClientDataCommand(client.Id);

        _clientRepository.GetByIdAsync(client.Id, Arg.Any<CancellationToken>()).Returns(client);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        client.IsAnonymized.Should().BeTrue();
        _clientRepository.Received(1).Update(client);

        await _dataDeletionRequestRepository.Received(1).AddAsync(
            Arg.Is<DataDeletionRequest>(r =>
                r.ClientId == client.Id &&
                r.TenantId == _tenantId &&
                r.Status == DeletionRequestStatus.Completed),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NonExistentClient_ShouldReturnNotFound()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var command = new DeleteClientDataCommand(clientId);

        _clientRepository.GetByIdAsync(clientId, Arg.Any<CancellationToken>()).Returns((Client?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ClientErrors.NotFound);

        _clientRepository.DidNotReceive().Update(Arg.Any<Client>());
        await _dataDeletionRequestRepository.DidNotReceive().AddAsync(Arg.Any<DataDeletionRequest>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AlreadyAnonymizedClient_ShouldReturnAlreadyAnonymized()
    {
        // Arrange
        var client = CreateClient();
        client.Anonymize();

        var command = new DeleteClientDataCommand(client.Id);

        _clientRepository.GetByIdAsync(client.Id, Arg.Any<CancellationToken>()).Returns(client);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ClientErrors.AlreadyAnonymized);

        _clientRepository.DidNotReceive().Update(Arg.Any<Client>());
        await _dataDeletionRequestRepository.DidNotReceive().AddAsync(Arg.Any<DataDeletionRequest>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
