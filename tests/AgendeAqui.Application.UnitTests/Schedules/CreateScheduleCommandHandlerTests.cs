using AgendeAqui.Application.Schedules.CreateSchedule;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Professionals;
using AgendeAqui.Domain.Schedules;
using AgendeAqui.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Schedules;

public class CreateScheduleCommandHandlerTests
{
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IProfessionalRepository _professionalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly CreateScheduleCommandHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public CreateScheduleCommandHandlerTests()
    {
        _scheduleRepository = Substitute.For<IScheduleRepository>();
        _professionalRepository = Substitute.For<IProfessionalRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _tenantProvider.GetTenantId().Returns(_tenantId);

        _handler = new CreateScheduleCommandHandler(
            _scheduleRepository,
            _professionalRepository,
            _unitOfWork,
            _tenantProvider);
    }

    private Professional CreateProfessional() =>
        Professional.Create(_tenantId, "John", Email.Create("john@test.com").Value, PhoneNumber.Create("5511987654321").Value).Value;

    private static CreateScheduleCommand BuildCommand(Guid professionalId) =>
        new(professionalId, DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(18, 0), 30);

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateScheduleAndReturnId()
    {
        var professional = CreateProfessional();
        _professionalRepository.GetByIdAsync(professional.Id, Arg.Any<CancellationToken>()).Returns(professional);
        _scheduleRepository.GetByProfessionalAndDayAsync(professional.Id, DayOfWeek.Monday, Arg.Any<CancellationToken>()).Returns((Schedule?)null);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = BuildCommand(professional.Id);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _scheduleRepository.Received(1).AddAsync(
            Arg.Is<Schedule>(s => s.ProfessionalId == professional.Id && s.TenantId == _tenantId),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentProfessional_ShouldReturnProfessionalNotFound()
    {
        _professionalRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Professional?)null);

        var command = BuildCommand(Guid.NewGuid());
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ScheduleErrors.ProfessionalNotFound);
        await _scheduleRepository.DidNotReceive().AddAsync(Arg.Any<Schedule>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInactiveProfessional_ShouldReturnProfessionalInactive()
    {
        var professional = CreateProfessional();
        professional.Deactivate();
        _professionalRepository.GetByIdAsync(professional.Id, Arg.Any<CancellationToken>()).Returns(professional);

        var command = BuildCommand(professional.Id);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ScheduleErrors.ProfessionalInactive);
        await _scheduleRepository.DidNotReceive().AddAsync(Arg.Any<Schedule>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDuplicateDay_ShouldReturnDuplicateDay()
    {
        var professional = CreateProfessional();
        var existingSchedule = Schedule.Create(_tenantId, professional.Id, DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(18, 0), TimeSpan.FromMinutes(30)).Value;

        _professionalRepository.GetByIdAsync(professional.Id, Arg.Any<CancellationToken>()).Returns(professional);
        _scheduleRepository.GetByProfessionalAndDayAsync(professional.Id, DayOfWeek.Monday, Arg.Any<CancellationToken>()).Returns(existingSchedule);

        var command = BuildCommand(professional.Id);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ScheduleErrors.DuplicateDay);
        await _scheduleRepository.DidNotReceive().AddAsync(Arg.Any<Schedule>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidTimes_ShouldReturnInvalidSchedule()
    {
        var professional = CreateProfessional();
        _professionalRepository.GetByIdAsync(professional.Id, Arg.Any<CancellationToken>()).Returns(professional);
        _scheduleRepository.GetByProfessionalAndDayAsync(professional.Id, DayOfWeek.Monday, Arg.Any<CancellationToken>()).Returns((Schedule?)null);

        var command = new CreateScheduleCommand(professional.Id, DayOfWeek.Monday, new TimeOnly(18, 0), new TimeOnly(8, 0), 30);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ScheduleErrors.InvalidSchedule);
        await _scheduleRepository.DidNotReceive().AddAsync(Arg.Any<Schedule>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
