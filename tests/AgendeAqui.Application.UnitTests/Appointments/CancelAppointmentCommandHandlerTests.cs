using AgendeAqui.Application.Appointments.CancelAppointment;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments;
using AgendeAqui.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Appointments;

public class CancelAppointmentCommandHandlerTests
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CancelAppointmentCommandHandler _handler;

    public CancelAppointmentCommandHandlerTests()
    {
        _appointmentRepository = Substitute.For<IAppointmentRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CancelAppointmentCommandHandler(_appointmentRepository, _unitOfWork);
    }

    private static Appointment CreateAppointment() =>
        Appointment.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            TimeSlot.Create(new TimeOnly(9, 0), new TimeOnly(10, 0)).Value).Value;

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCancelAppointment()
    {
        var appointment = CreateAppointment();
        _appointmentRepository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = new CancelAppointmentCommand(appointment.Id, "Patient request");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
        _appointmentRepository.Received(1).Update(appointment);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentAppointment_ShouldReturnNotFound()
    {
        _appointmentRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Appointment?)null);

        var command = new CancelAppointmentCommand(Guid.NewGuid(), "Reason");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AppointmentErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WhenAlreadyCompleted_ShouldReturnInvalidTransition()
    {
        var appointment = CreateAppointment();
        appointment.Confirm();
        appointment.Start();
        appointment.Complete();
        _appointmentRepository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);

        var command = new CancelAppointmentCommand(appointment.Id, "Too late");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AppointmentErrors.InvalidTransition);
    }
}
