using AgendeAqui.Application.Appointments.RescheduleAppointment;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments;
using AgendeAqui.Domain.Schedules;
using AgendeAqui.Domain.Services;
using AgendeAqui.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Appointments;

public class RescheduleAppointmentCommandHandlerTests
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly RescheduleAppointmentCommandHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public RescheduleAppointmentCommandHandlerTests()
    {
        _appointmentRepository = Substitute.For<IAppointmentRepository>();
        _serviceRepository = Substitute.For<IServiceRepository>();
        _scheduleRepository = Substitute.For<IScheduleRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _currentUser = Substitute.For<ICurrentUser>();
        _currentUser.Role.Returns("Admin");
        _currentUser.IsAdmin.Returns(true);
        _handler = new RescheduleAppointmentCommandHandler(_appointmentRepository, _serviceRepository, _scheduleRepository, _unitOfWork, _currentUser);
    }

    private Appointment CreateAppointment(Guid? professionalId = null) =>
        Appointment.Create(_tenantId, professionalId ?? Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            TimeSlot.Create(new TimeOnly(9, 0), new TimeOnly(10, 0)).Value).Value;

    private Service CreateService() =>
        Service.Create(_tenantId, "Haircut", TimeSpan.FromMinutes(60), 50m).Value;

    private Schedule CreateSchedule(Guid professionalId, DayOfWeek day) =>
        Schedule.Create(_tenantId, professionalId, day, new TimeOnly(8, 0), new TimeOnly(18, 0), TimeSpan.FromMinutes(30)).Value;

    [Fact]
    public async Task Handle_WithValidCommand_ShouldRescheduleAppointment()
    {
        var professionalId = Guid.NewGuid();
        var appointment = CreateAppointment(professionalId);
        var service = CreateService();
        var newDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
        while (newDate.DayOfWeek != DayOfWeek.Tuesday) newDate = newDate.AddDays(1);
        var schedule = CreateSchedule(professionalId, DayOfWeek.Tuesday);

        _appointmentRepository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);
        _serviceRepository.GetByIdAsync(appointment.ServiceId, Arg.Any<CancellationToken>()).Returns(service);
        _scheduleRepository.GetByProfessionalAndDayAsync(professionalId, DayOfWeek.Tuesday, Arg.Any<CancellationToken>()).Returns(schedule);
        _appointmentRepository.HasConflictAsync(Arg.Any<Guid>(), Arg.Any<DateOnly>(), Arg.Any<TimeOnly>(), Arg.Any<TimeOnly>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns(false);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = new RescheduleAppointmentCommand(appointment.Id, newDate, new TimeOnly(14, 0));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        appointment.Date.Should().Be(newDate);
        appointment.TimeSlot.Start.Should().Be(new TimeOnly(14, 0));
        _appointmentRepository.Received(1).Update(appointment);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentAppointment_ShouldReturnNotFound()
    {
        _appointmentRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Appointment?)null);

        var command = new RescheduleAppointmentCommand(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today.AddDays(5)), new TimeOnly(14, 0));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AppointmentErrors.NotFound);
    }

    [Fact]
    public async Task Handle_WithServiceNotFound_ShouldReturnFailure()
    {
        var appointment = CreateAppointment();
        _appointmentRepository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);
        _serviceRepository.GetByIdAsync(appointment.ServiceId, Arg.Any<CancellationToken>()).Returns((Service?)null);

        var command = new RescheduleAppointmentCommand(appointment.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(5)), new TimeOnly(14, 0));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AppointmentErrors.ServiceNotFound);
    }

    [Fact]
    public async Task Handle_WithNoSchedule_ShouldReturnFailure()
    {
        var professionalId = Guid.NewGuid();
        var appointment = CreateAppointment(professionalId);
        var service = CreateService();

        _appointmentRepository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);
        _serviceRepository.GetByIdAsync(appointment.ServiceId, Arg.Any<CancellationToken>()).Returns(service);
        _scheduleRepository.GetByProfessionalAndDayAsync(professionalId, Arg.Any<DayOfWeek>(), Arg.Any<CancellationToken>()).Returns((Schedule?)null);

        var command = new RescheduleAppointmentCommand(appointment.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(5)), new TimeOnly(14, 0));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AppointmentErrors.NoSchedule);
    }

    [Fact]
    public async Task Handle_OutsideSchedule_ShouldReturnFailure()
    {
        var professionalId = Guid.NewGuid();
        var appointment = CreateAppointment(professionalId);
        var service = CreateService();
        var newDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
        while (newDate.DayOfWeek != DayOfWeek.Tuesday) newDate = newDate.AddDays(1);
        var schedule = CreateSchedule(professionalId, DayOfWeek.Tuesday);

        _appointmentRepository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);
        _serviceRepository.GetByIdAsync(appointment.ServiceId, Arg.Any<CancellationToken>()).Returns(service);
        _scheduleRepository.GetByProfessionalAndDayAsync(professionalId, DayOfWeek.Tuesday, Arg.Any<CancellationToken>()).Returns(schedule);

        var command = new RescheduleAppointmentCommand(appointment.Id, newDate, new TimeOnly(6, 0));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AppointmentErrors.OutsideSchedule);
    }

    [Fact]
    public async Task Handle_WithConflict_ShouldReturnFailure()
    {
        var professionalId = Guid.NewGuid();
        var appointment = CreateAppointment(professionalId);
        var service = CreateService();
        var newDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
        while (newDate.DayOfWeek != DayOfWeek.Tuesday) newDate = newDate.AddDays(1);
        var schedule = CreateSchedule(professionalId, DayOfWeek.Tuesday);

        _appointmentRepository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);
        _serviceRepository.GetByIdAsync(appointment.ServiceId, Arg.Any<CancellationToken>()).Returns(service);
        _scheduleRepository.GetByProfessionalAndDayAsync(professionalId, DayOfWeek.Tuesday, Arg.Any<CancellationToken>()).Returns(schedule);
        _appointmentRepository.HasConflictAsync(Arg.Any<Guid>(), Arg.Any<DateOnly>(), Arg.Any<TimeOnly>(), Arg.Any<TimeOnly>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns(true);

        var command = new RescheduleAppointmentCommand(appointment.Id, newDate, new TimeOnly(14, 0));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AppointmentErrors.Conflict);
    }
}
