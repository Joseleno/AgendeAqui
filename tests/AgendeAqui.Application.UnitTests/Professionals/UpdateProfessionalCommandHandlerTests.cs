using AgendeAqui.Application.Professionals.UpdateProfessional;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Professionals;
using AgendeAqui.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Professionals;

public class UpdateProfessionalCommandHandlerTests
{
    private readonly IProfessionalRepository _professionalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateProfessionalCommandHandler _handler;

    public UpdateProfessionalCommandHandlerTests()
    {
        _professionalRepository = Substitute.For<IProfessionalRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new UpdateProfessionalCommandHandler(_professionalRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateProfessional()
    {
        var professional = Professional.Create(
            Guid.NewGuid(), "John Doe",
            Email.Create("john@example.com").Value,
            PhoneNumber.Create("5511987654321").Value).Value;

        _professionalRepository.GetByIdAsync(professional.Id, Arg.Any<CancellationToken>())
            .Returns(professional);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = new UpdateProfessionalCommand(professional.Id, "Jane Doe", "jane@example.com", "5521912345678");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        professional.Name.Should().Be("Jane Doe");
        _professionalRepository.Received(1).Update(professional);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentProfessional_ShouldReturnNotFound()
    {
        _professionalRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Professional?)null);

        var command = new UpdateProfessionalCommand(Guid.NewGuid(), "Jane", "jane@example.com", "5521912345678");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProfessionalErrors.NotFound);
        _professionalRepository.DidNotReceive().Update(Arg.Any<Professional>());
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldReturnFailure()
    {
        var professional = Professional.Create(
            Guid.NewGuid(), "John",
            Email.Create("john@example.com").Value,
            PhoneNumber.Create("5511987654321").Value).Value;

        _professionalRepository.GetByIdAsync(professional.Id, Arg.Any<CancellationToken>())
            .Returns(professional);

        var command = new UpdateProfessionalCommand(professional.Id, "Jane", "invalid", "5521912345678");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Email.InvalidEmail);
    }

    [Fact]
    public async Task Handle_WithEmptyName_ShouldReturnFailure()
    {
        var professional = Professional.Create(
            Guid.NewGuid(), "John",
            Email.Create("john@example.com").Value,
            PhoneNumber.Create("5511987654321").Value).Value;

        _professionalRepository.GetByIdAsync(professional.Id, Arg.Any<CancellationToken>())
            .Returns(professional);

        var command = new UpdateProfessionalCommand(professional.Id, "", "jane@example.com", "5521912345678");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProfessionalErrors.InvalidName);
    }

    [Fact]
    public async Task Handle_WithInvalidPhone_ShouldReturnFailure()
    {
        var professional = Professional.Create(
            Guid.NewGuid(), "John",
            Email.Create("john@example.com").Value,
            PhoneNumber.Create("5511987654321").Value).Value;

        _professionalRepository.GetByIdAsync(professional.Id, Arg.Any<CancellationToken>())
            .Returns(professional);

        var command = new UpdateProfessionalCommand(professional.Id, "Jane", "jane@example.com", "invalid");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PhoneNumber.InvalidPhone);
    }
}
