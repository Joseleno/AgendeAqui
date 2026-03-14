using AgendeAqui.Application.Absences.CreateAbsence;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Professionals;
using AgendeAqui.Domain.Schedules;
using AgendeAqui.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Absences;

public class CreateAbsenceCommandHandlerTests
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IProfessionalRepository _professionalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUser _currentUser;
    private readonly CreateAbsenceCommandHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public CreateAbsenceCommandHandlerTests()
    {
        _absenceRepository = Substitute.For<IAbsenceRepository>();
        _professionalRepository = Substitute.For<IProfessionalRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _currentUser = Substitute.For<ICurrentUser>();
        _tenantProvider.GetTenantId().Returns(_tenantId);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Default to admin so existing tests pass without hitting authorization logic
        _currentUser.IsAdmin.Returns(true);

        _handler = new CreateAbsenceCommandHandler(
            _absenceRepository,
            _professionalRepository,
            _unitOfWork,
            _tenantProvider,
            _currentUser);
    }

    private Professional CreateProfessional() =>
        Professional.Create(_tenantId, "John", Email.Create("john@test.com").Value, PhoneNumber.Create("5511987654321").Value).Value;

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateAbsenceAndReturnId()
    {
        var professional = CreateProfessional();
        _professionalRepository.GetByIdAsync(professional.Id, Arg.Any<CancellationToken>()).Returns(professional);

        var command = new CreateAbsenceCommand(
            professional.Id,
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            new TimeOnly(9, 0),
            new TimeOnly(12, 0),
            "Doctor appointment");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _absenceRepository.Received(1).AddAsync(Arg.Any<Absence>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentProfessional_ShouldReturnFailure()
    {
        _professionalRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Professional?)null);

        var command = new CreateAbsenceCommand(
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            new TimeOnly(9, 0),
            new TimeOnly(12, 0),
            "Doctor appointment");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProfessionalErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WithInvalidTimeRange_ShouldReturnFailure()
    {
        var professional = CreateProfessional();
        _professionalRepository.GetByIdAsync(professional.Id, Arg.Any<CancellationToken>()).Returns(professional);

        var command = new CreateAbsenceCommand(
            professional.Id,
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            new TimeOnly(14, 0),
            new TimeOnly(10, 0),
            "Invalid range");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AbsenceErrors.InvalidTimeRange);
    }

    [Fact]
    public async Task Handle_WithFullDayAbsence_ShouldSucceed()
    {
        var professional = CreateProfessional();
        _professionalRepository.GetByIdAsync(professional.Id, Arg.Any<CancellationToken>()).Returns(professional);

        var command = new CreateAbsenceCommand(
            professional.Id,
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            null,
            null,
            "Vacation day");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _absenceRepository.Received(1).AddAsync(
            Arg.Is<Absence>(a => a.IsFullDay),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
