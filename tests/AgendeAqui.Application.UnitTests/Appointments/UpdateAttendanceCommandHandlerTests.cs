using AgendeAqui.Application.Appointments.UpdateAttendance;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments;
using AgendeAqui.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Appointments;

public class UpdateAttendanceCommandHandlerTests
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateAttendanceCommandHandler _handler;

    public UpdateAttendanceCommandHandlerTests()
    {
        _appointmentRepository = Substitute.For<IAppointmentRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new UpdateAttendanceCommandHandler(_appointmentRepository, _unitOfWork);
    }

    private static Appointment CreateAppointment() =>
        Appointment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            TimeSlot.Create(new TimeOnly(9, 0), new TimeOnly(10, 0)).Value).Value;

    [Fact]
    public async Task Handle_Confirm_FromScheduled_ShouldSucceed()
    {
        var appointment = CreateAppointment();
        _appointmentRepository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new UpdateAttendanceCommand(appointment.Id, AttendanceAction.Confirm), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        appointment.Status.Should().Be(AppointmentStatus.Confirmed);
        _appointmentRepository.Received(1).Update(appointment);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Start_FromConfirmed_ShouldSucceed()
    {
        var appointment = CreateAppointment();
        appointment.Confirm();
        _appointmentRepository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new UpdateAttendanceCommand(appointment.Id, AttendanceAction.Start), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        appointment.Status.Should().Be(AppointmentStatus.InProgress);
        _appointmentRepository.Received(1).Update(appointment);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Complete_FromInProgress_ShouldSucceed()
    {
        var appointment = CreateAppointment();
        appointment.Confirm();
        appointment.Start();
        _appointmentRepository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new UpdateAttendanceCommand(appointment.Id, AttendanceAction.Complete), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        appointment.Status.Should().Be(AppointmentStatus.Completed);
        _appointmentRepository.Received(1).Update(appointment);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoShow_FromInProgress_ShouldSucceed()
    {
        var appointment = CreateAppointment();
        appointment.Confirm();
        appointment.Start();
        _appointmentRepository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new UpdateAttendanceCommand(appointment.Id, AttendanceAction.NoShow), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        appointment.Status.Should().Be(AppointmentStatus.NoShow);
        _appointmentRepository.Received(1).Update(appointment);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Start_FromScheduled_ShouldFail()
    {
        var appointment = CreateAppointment();
        _appointmentRepository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);

        var result = await _handler.Handle(new UpdateAttendanceCommand(appointment.Id, AttendanceAction.Start), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AppointmentErrors.InvalidTransition);
    }

    [Fact]
    public async Task Handle_Complete_FromScheduled_ShouldFail()
    {
        var appointment = CreateAppointment();
        _appointmentRepository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);

        var result = await _handler.Handle(new UpdateAttendanceCommand(appointment.Id, AttendanceAction.Complete), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AppointmentErrors.InvalidTransition);
    }

    [Fact]
    public async Task Handle_WithNonExistentAppointment_ShouldReturnNotFound()
    {
        _appointmentRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Appointment?)null);

        var result = await _handler.Handle(new UpdateAttendanceCommand(Guid.NewGuid(), AttendanceAction.Confirm), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AppointmentErrors.NotFound);
    }
}
