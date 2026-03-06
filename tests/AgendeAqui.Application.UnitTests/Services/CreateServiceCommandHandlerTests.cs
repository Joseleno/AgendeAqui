using AgendeAqui.Application.Services.CreateService;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Services;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Services;

public class CreateServiceCommandHandlerTests
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly CreateServiceCommandHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public CreateServiceCommandHandlerTests()
    {
        _serviceRepository = Substitute.For<IServiceRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _tenantProvider.GetTenantId().Returns(_tenantId);
        _handler = new CreateServiceCommandHandler(_serviceRepository, _unitOfWork, _tenantProvider);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateServiceAndReturnId()
    {
        var command = new CreateServiceCommand("Haircut", 30, 50m);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _serviceRepository.Received(1).AddAsync(Arg.Any<Service>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptyName_ShouldReturnFailure()
    {
        var command = new CreateServiceCommand("", 30, 50m);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.InvalidName);
        await _serviceRepository.DidNotReceive().AddAsync(Arg.Any<Service>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithZeroDuration_ShouldReturnFailure()
    {
        var command = new CreateServiceCommand("Haircut", 0, 50m);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.InvalidDuration);
    }

    [Fact]
    public async Task Handle_WithNegativePrice_ShouldReturnFailure()
    {
        var command = new CreateServiceCommand("Haircut", 30, -10m);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ServiceErrors.InvalidPrice);
    }
}
