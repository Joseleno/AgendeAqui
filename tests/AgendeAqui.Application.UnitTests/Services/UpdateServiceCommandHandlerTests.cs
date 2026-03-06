using AgendeAqui.Application.Services.UpdateService;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Services;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Services;

public class UpdateServiceCommandHandlerTests
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateServiceCommandHandler _handler;

    public UpdateServiceCommandHandlerTests()
    {
        _serviceRepository = Substitute.For<IServiceRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new UpdateServiceCommandHandler(_serviceRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateService()
    {
        var service = Service.Create(Guid.NewGuid(), "Haircut", TimeSpan.FromMinutes(30), 50m).Value;

        _serviceRepository.GetByIdAsync(service.Id, Arg.Any<CancellationToken>())
            .Returns(service);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = new UpdateServiceCommand(service.Id, "Premium Cut", 60, 100m);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        service.Name.Should().Be("Premium Cut");
        service.Duration.Should().Be(TimeSpan.FromMinutes(60));
        service.Price.Should().Be(100m);
        _serviceRepository.Received(1).Update(service);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentService_ShouldReturnNotFound()
    {
        _serviceRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Service?)null);

        var command = new UpdateServiceCommand(Guid.NewGuid(), "Cut", 30, 50m);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.NotFound);
        _serviceRepository.DidNotReceive().Update(Arg.Any<Service>());
    }

    [Fact]
    public async Task Handle_WithEmptyName_ShouldReturnFailure()
    {
        var service = Service.Create(Guid.NewGuid(), "Haircut", TimeSpan.FromMinutes(30), 50m).Value;

        _serviceRepository.GetByIdAsync(service.Id, Arg.Any<CancellationToken>())
            .Returns(service);

        var command = new UpdateServiceCommand(service.Id, "", 30, 50m);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.InvalidName);
    }

    [Fact]
    public async Task Handle_WithNegativePrice_ShouldReturnFailure()
    {
        var service = Service.Create(Guid.NewGuid(), "Haircut", TimeSpan.FromMinutes(30), 50m).Value;

        _serviceRepository.GetByIdAsync(service.Id, Arg.Any<CancellationToken>())
            .Returns(service);

        var command = new UpdateServiceCommand(service.Id, "Haircut", 30, -10m);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.InvalidPrice);
    }
}
