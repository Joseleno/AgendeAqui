using AgendeAqui.Application.Tenants.CreateTenant;
using AgendeAqui.Application.Tenants.UpdateTenant;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Tenants;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Tenants;

public class UpdateTenantCommandHandlerTests
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateTenantCommandHandler _handler;

    public UpdateTenantCommandHandlerTests()
    {
        _tenantRepository = Substitute.For<ITenantRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new UpdateTenantCommandHandler(_tenantRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithExistingTenant_ShouldUpdateAndSave()
    {
        var tenant = Tenant.Create("Old Name", "test-tenant", TenantPlan.Free);
        var command = new UpdateTenantCommand(tenant.Id, "New Name", "Professional");

        _tenantRepository.GetByIdAsync(tenant.Id, Arg.Any<CancellationToken>()).Returns(tenant);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        tenant.Name.Should().Be("New Name");
        tenant.Plan.Should().Be(TenantPlan.Professional);
        _tenantRepository.Received(1).Update(tenant);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentTenant_ShouldReturnNotFound()
    {
        var command = new UpdateTenantCommand(Guid.NewGuid(), "Name", "Free");
        _tenantRepository.GetByIdAsync(command.TenantId, Arg.Any<CancellationToken>()).Returns((Tenant?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TenantErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WithPlanChange_ShouldUpdatePlan()
    {
        var tenant = Tenant.Create("Test", "test", TenantPlan.Free);
        var command = new UpdateTenantCommand(tenant.Id, "Test", "Enterprise");

        _tenantRepository.GetByIdAsync(tenant.Id, Arg.Any<CancellationToken>()).Returns(tenant);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        tenant.Plan.Should().Be(TenantPlan.Enterprise);
    }

    [Fact]
    public async Task Handle_WithNonExistentTenant_ShouldNotCallUpdateOrSave()
    {
        var command = new UpdateTenantCommand(Guid.NewGuid(), "Name", "Free");
        _tenantRepository.GetByIdAsync(command.TenantId, Arg.Any<CancellationToken>()).Returns((Tenant?)null);

        await _handler.Handle(command, CancellationToken.None);

        _tenantRepository.DidNotReceive().Update(Arg.Any<Tenant>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToRepositoryAndUoW()
    {
        var tenant = Tenant.Create("Test Corp", "test-corp", TenantPlan.Free);
        var command = new UpdateTenantCommand(tenant.Id, "Updated Corp", "Starter");
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        _tenantRepository.GetByIdAsync(tenant.Id, token).Returns(tenant);
        _unitOfWork.SaveChangesAsync(token).Returns(1);

        await _handler.Handle(command, token);

        await _tenantRepository.Received(1).GetByIdAsync(tenant.Id, token);
        await _unitOfWork.Received(1).SaveChangesAsync(token);
    }
}
