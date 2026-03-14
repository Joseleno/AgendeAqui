using AgendeAqui.Application.Availability.GetAvailability;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments;
using AgendeAqui.Domain.Schedules;
using AgendeAqui.Domain.Services;
using AgendeAqui.Domain.ValueObjects;
using Absence = AgendeAqui.Domain.Schedules.Absence;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Availability;

public class GetAvailabilityQueryHandlerTests
{
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly GetAvailabilityQueryHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public GetAvailabilityQueryHandlerTests()
    {
        _scheduleRepository = Substitute.For<IScheduleRepository>();
        _appointmentRepository = Substitute.For<IAppointmentRepository>();
        _serviceRepository = Substitute.For<IServiceRepository>();
        _absenceRepository = Substitute.For<IAbsenceRepository>();
        _absenceRepository.GetByProfessionalAndDateAsync(Arg.Any<Guid>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(new List<Absence>());
        _handler = new GetAvailabilityQueryHandler(_scheduleRepository, _appointmentRepository, _serviceRepository, _absenceRepository);
    }

    [Fact]
    public async Task Handle_WithNoSchedule_ShouldReturnEmptySlots()
    {
        var service = Service.Create(_tenantId, "Haircut", TimeSpan.FromMinutes(30), 50m).Value;
        var professionalId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

        _serviceRepository.GetByIdAsync(service.Id, Arg.Any<CancellationToken>()).Returns(service);
        _scheduleRepository.GetByProfessionalAndDayAsync(professionalId, Arg.Any<DayOfWeek>(), Arg.Any<CancellationToken>()).Returns((Schedule?)null);

        var query = new GetAvailabilityQuery(professionalId, date, service.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Slots.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithScheduleAndNoAppointments_ShouldReturnAllSlots()
    {
        var professionalId = Guid.NewGuid();
        var service = Service.Create(_tenantId, "Haircut", TimeSpan.FromMinutes(30), 50m).Value;
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        while (date.DayOfWeek != DayOfWeek.Monday) date = date.AddDays(1);

        var schedule = Schedule.Create(_tenantId, professionalId, DayOfWeek.Monday,
            new TimeOnly(9, 0), new TimeOnly(11, 0), TimeSpan.FromMinutes(30)).Value;

        _serviceRepository.GetByIdAsync(service.Id, Arg.Any<CancellationToken>()).Returns(service);
        _scheduleRepository.GetByProfessionalAndDayAsync(professionalId, DayOfWeek.Monday, Arg.Any<CancellationToken>()).Returns(schedule);
        _appointmentRepository.GetByProfessionalAndDateAsync(professionalId, date, Arg.Any<CancellationToken>())
            .Returns(new List<Appointment>());

        var query = new GetAvailabilityQuery(professionalId, date, service.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Slots.Should().HaveCount(4); // 9:00, 9:30, 10:00, 10:30
    }

    [Fact]
    public async Task Handle_WithExistingAppointment_ShouldExcludeBookedSlots()
    {
        var professionalId = Guid.NewGuid();
        var service = Service.Create(_tenantId, "Haircut", TimeSpan.FromMinutes(30), 50m).Value;
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        while (date.DayOfWeek != DayOfWeek.Monday) date = date.AddDays(1);

        var schedule = Schedule.Create(_tenantId, professionalId, DayOfWeek.Monday,
            new TimeOnly(9, 0), new TimeOnly(11, 0), TimeSpan.FromMinutes(30)).Value;

        var existingAppointment = Appointment.Create(
            _tenantId, professionalId, Guid.NewGuid(), Guid.NewGuid(), date,
            TimeSlot.Create(new TimeOnly(9, 0), new TimeOnly(9, 30)).Value).Value;

        _serviceRepository.GetByIdAsync(service.Id, Arg.Any<CancellationToken>()).Returns(service);
        _scheduleRepository.GetByProfessionalAndDayAsync(professionalId, DayOfWeek.Monday, Arg.Any<CancellationToken>()).Returns(schedule);
        _appointmentRepository.GetByProfessionalAndDateAsync(professionalId, date, Arg.Any<CancellationToken>())
            .Returns(new List<Appointment> { existingAppointment });

        var query = new GetAvailabilityQuery(professionalId, date, service.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Slots.Should().HaveCount(3); // 9:30, 10:00, 10:30 (9:00 booked)
        result.Value.Slots.Should().NotContain(s => s.Start == new TimeOnly(9, 0));
    }

    [Fact]
    public async Task Handle_WithCancelledAppointment_ShouldNotExcludeSlot()
    {
        var professionalId = Guid.NewGuid();
        var service = Service.Create(_tenantId, "Haircut", TimeSpan.FromMinutes(30), 50m).Value;
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        while (date.DayOfWeek != DayOfWeek.Monday) date = date.AddDays(1);

        var schedule = Schedule.Create(_tenantId, professionalId, DayOfWeek.Monday,
            new TimeOnly(9, 0), new TimeOnly(11, 0), TimeSpan.FromMinutes(30)).Value;

        var cancelledAppointment = Appointment.Create(
            _tenantId, professionalId, Guid.NewGuid(), Guid.NewGuid(), date,
            TimeSlot.Create(new TimeOnly(9, 0), new TimeOnly(9, 30)).Value).Value;
        cancelledAppointment.Cancel("No longer needed");

        _serviceRepository.GetByIdAsync(service.Id, Arg.Any<CancellationToken>()).Returns(service);
        _scheduleRepository.GetByProfessionalAndDayAsync(professionalId, DayOfWeek.Monday, Arg.Any<CancellationToken>()).Returns(schedule);
        _appointmentRepository.GetByProfessionalAndDateAsync(professionalId, date, Arg.Any<CancellationToken>())
            .Returns(new List<Appointment> { cancelledAppointment });

        var query = new GetAvailabilityQuery(professionalId, date, service.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Slots.Should().HaveCount(4); // All 4 slots available since appointment was cancelled
        result.Value.Slots.Should().Contain(s => s.Start == new TimeOnly(9, 0));
    }

    [Fact]
    public async Task Handle_WithNonExistentService_ShouldReturnFailure()
    {
        _serviceRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Service?)null);

        var query = new GetAvailabilityQuery(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today.AddDays(1)), Guid.NewGuid());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AppointmentErrors.ServiceNotFound);
    }
}
