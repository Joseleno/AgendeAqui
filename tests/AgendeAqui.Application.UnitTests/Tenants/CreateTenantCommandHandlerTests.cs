using AgendeAqui.Application.Tenants.CreateTenant;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Tenants;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Tenants;

public class CreateTenantCommandHandlerTests
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateTenantCommandHandler _handler;

    public CreateTenantCommandHandlerTests()
    {
        _tenantRepository = Substitute.For<ITenantRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CreateTenantCommandHandler(_tenantRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateTenantAndReturnId()
    {
        var command = new CreateTenantCommand("Acme Corp", "acme-corp", "Starter");

        _tenantRepository.GetBySlugAsync(command.Slug, Arg.Any<CancellationToken>())
            .Returns((Tenant?)null);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        await _tenantRepository.Received(1).AddAsync(Arg.Any<Tenant>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSlugAlreadyExists_ShouldReturnFailure()
    {
        var command = new CreateTenantCommand("Acme Corp", "acme-corp", "Free");
        var existingTenant = Tenant.Create("Existing Corp", "acme-corp");

        _tenantRepository.GetBySlugAsync(command.Slug, Arg.Any<CancellationToken>())
            .Returns(existingTenant);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TenantErrors.SlugAlreadyExists);

        await _tenantRepository.DidNotReceive().AddAsync(Arg.Any<Tenant>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithValidPlan_ShouldCreateTenantWithCorrectPlan()
    {
        var command = new CreateTenantCommand("Pro Corp", "pro-corp", "Professional");

        _tenantRepository.GetBySlugAsync(command.Slug, Arg.Any<CancellationToken>())
            .Returns((Tenant?)null);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        Tenant? capturedTenant = null;
        await _tenantRepository.AddAsync(
            Arg.Do<Tenant>(t => capturedTenant = t),
            Arg.Any<CancellationToken>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        capturedTenant.Should().NotBeNull();
        capturedTenant!.Plan.Should().Be(TenantPlan.Professional);
        capturedTenant.Slug.Should().Be("pro-corp");
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToRepositoryAndUoW()
    {
        var command = new CreateTenantCommand("Test Corp", "test-corp", "Free");
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        _tenantRepository.GetBySlugAsync(command.Slug, token)
            .Returns((Tenant?)null);

        _unitOfWork.SaveChangesAsync(token).Returns(1);

        await _handler.Handle(command, token);

        await _tenantRepository.Received(1).GetBySlugAsync(command.Slug, token);
        await _unitOfWork.Received(1).SaveChangesAsync(token);
    }
}
