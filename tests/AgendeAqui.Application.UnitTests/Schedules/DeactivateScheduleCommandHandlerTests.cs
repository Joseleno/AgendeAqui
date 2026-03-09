using AgendeAqui.Application.Schedules.DeactivateSchedule;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Schedules;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Schedules;

public class DeactivateScheduleCommandHandlerTests
{
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeactivateScheduleCommandHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public DeactivateScheduleCommandHandlerTests()
    {
        _scheduleRepository = Substitute.For<IScheduleRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new DeactivateScheduleCommandHandler(_scheduleRepository, _unitOfWork);
    }

    private Schedule CreateSchedule() =>
        Schedule.Create(_tenantId, Guid.NewGuid(), DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(18, 0), TimeSpan.FromMinutes(30)).Value;

    [Fact]
    public async Task Handle_WithExistingSchedule_ShouldDeactivateAndSave()
    {
        var schedule = CreateSchedule();
        schedule.IsActive.Should().BeTrue();
        _scheduleRepository.GetByIdAsync(schedule.Id, Arg.Any<CancellationToken>()).Returns(schedule);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = new DeactivateScheduleCommand(schedule.Id);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        schedule.IsActive.Should().BeFalse();
        _scheduleRepository.Received(1).Update(schedule);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentSchedule_ShouldReturnNotFound()
    {
        _scheduleRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Schedule?)null);

        var command = new DeactivateScheduleCommand(Guid.NewGuid());
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ScheduleErrors.NotFound);
        _scheduleRepository.DidNotReceive().Update(Arg.Any<Schedule>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithAlreadyInactiveSchedule_ShouldStillDeactivateAndSave()
    {
        var schedule = CreateSchedule();
        schedule.Deactivate();
        schedule.IsActive.Should().BeFalse();
        _scheduleRepository.GetByIdAsync(schedule.Id, Arg.Any<CancellationToken>()).Returns(schedule);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = new DeactivateScheduleCommand(schedule.Id);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        schedule.IsActive.Should().BeFalse();
        _scheduleRepository.Received(1).Update(schedule);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
