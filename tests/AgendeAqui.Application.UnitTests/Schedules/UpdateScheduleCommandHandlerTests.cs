using AgendeAqui.Application.Schedules.UpdateSchedule;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Schedules;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Schedules;

public class UpdateScheduleCommandHandlerTests
{
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateScheduleCommandHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public UpdateScheduleCommandHandlerTests()
    {
        _scheduleRepository = Substitute.For<IScheduleRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new UpdateScheduleCommandHandler(_scheduleRepository, _unitOfWork);
    }

    private Schedule CreateSchedule() =>
        Schedule.Create(_tenantId, Guid.NewGuid(), DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(18, 0), TimeSpan.FromMinutes(30)).Value;

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateSchedule()
    {
        var schedule = CreateSchedule();
        _scheduleRepository.GetByIdAsync(schedule.Id, Arg.Any<CancellationToken>()).Returns(schedule);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = new UpdateScheduleCommand(schedule.Id, new TimeOnly(9, 0), new TimeOnly(17, 0), 60);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        schedule.StartTime.Should().Be(new TimeOnly(9, 0));
        schedule.EndTime.Should().Be(new TimeOnly(17, 0));
        schedule.SlotDuration.Should().Be(TimeSpan.FromMinutes(60));
        _scheduleRepository.Received(1).Update(schedule);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentSchedule_ShouldReturnNotFound()
    {
        _scheduleRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Schedule?)null);

        var command = new UpdateScheduleCommand(Guid.NewGuid(), new TimeOnly(9, 0), new TimeOnly(17, 0), 60);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ScheduleErrors.NotFound);
        _scheduleRepository.DidNotReceive().Update(Arg.Any<Schedule>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithStartTimeAfterEndTime_ShouldReturnInvalidSchedule()
    {
        var schedule = CreateSchedule();
        _scheduleRepository.GetByIdAsync(schedule.Id, Arg.Any<CancellationToken>()).Returns(schedule);

        var command = new UpdateScheduleCommand(schedule.Id, new TimeOnly(18, 0), new TimeOnly(8, 0), 30);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ScheduleErrors.InvalidSchedule);
        _scheduleRepository.DidNotReceive().Update(Arg.Any<Schedule>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithSlotDurationLargerThanWindow_ShouldReturnInvalidSlotDuration()
    {
        var schedule = CreateSchedule();
        _scheduleRepository.GetByIdAsync(schedule.Id, Arg.Any<CancellationToken>()).Returns(schedule);

        var command = new UpdateScheduleCommand(schedule.Id, new TimeOnly(8, 0), new TimeOnly(9, 0), 120);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ScheduleErrors.InvalidSlotDuration);
        _scheduleRepository.DidNotReceive().Update(Arg.Any<Schedule>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
