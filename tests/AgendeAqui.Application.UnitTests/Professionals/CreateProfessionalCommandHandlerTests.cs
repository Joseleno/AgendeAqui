using AgendeAqui.Application.Professionals.CreateProfessional;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Professionals;
using AgendeAqui.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Professionals;

public class CreateProfessionalCommandHandlerTests
{
    private readonly IProfessionalRepository _professionalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly CreateProfessionalCommandHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public CreateProfessionalCommandHandlerTests()
    {
        _professionalRepository = Substitute.For<IProfessionalRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _tenantProvider.GetTenantId().Returns(_tenantId);
        _handler = new CreateProfessionalCommandHandler(_professionalRepository, _unitOfWork, _tenantProvider);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateProfessionalAndReturnId()
    {
        var command = new CreateProfessionalCommand("John Doe", "john@example.com", "5511987654321");
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _professionalRepository.Received(1).AddAsync(Arg.Any<Professional>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldReturnFailure()
    {
        var command = new CreateProfessionalCommand("John Doe", "invalid-email", "5511987654321");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Email.InvalidEmail);
        await _professionalRepository.DidNotReceive().AddAsync(Arg.Any<Professional>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidPhone_ShouldReturnFailure()
    {
        var command = new CreateProfessionalCommand("John Doe", "john@example.com", "invalid-phone");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PhoneNumber.InvalidPhone);
        await _professionalRepository.DidNotReceive().AddAsync(Arg.Any<Professional>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptyName_ShouldReturnFailure()
    {
        var command = new CreateProfessionalCommand("", "john@example.com", "5511987654321");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProfessionalErrors.InvalidName);
    }
}
