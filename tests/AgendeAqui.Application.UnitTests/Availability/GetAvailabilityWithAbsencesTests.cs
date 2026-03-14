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

public class GetAvailabilityWithAbsencesTests
{
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly GetAvailabilityQueryHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public GetAvailabilityWithAbsencesTests()
    {
        _scheduleRepository = Substitute.For<IScheduleRepository>();
        _appointmentRepository = Substitute.For<IAppointmentRepository>();
        _serviceRepository = Substitute.For<IServiceRepository>();
        _absenceRepository = Substitute.For<IAbsenceRepository>();
        _handler = new GetAvailabilityQueryHandler(_scheduleRepository, _appointmentRepository, _serviceRepository, _absenceRepository);
    }

    private DateOnly GetNextMonday()
    {
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        while (date.DayOfWeek != DayOfWeek.Monday) date = date.AddDays(1);
        return date;
    }

    [Fact]
    public async Task Handle_WithNoAbsences_ShouldReturnAllAvailableSlots()
    {
        var professionalId = Guid.NewGuid();
        var service = Service.Create(_tenantId, "Haircut", TimeSpan.FromMinutes(30), 50m).Value;
        var date = GetNextMonday();

        var schedule = Schedule.Create(_tenantId, professionalId, DayOfWeek.Monday,
            new TimeOnly(9, 0), new TimeOnly(13, 0), TimeSpan.FromMinutes(30)).Value;

        _serviceRepository.GetByIdAsync(service.Id, Arg.Any<CancellationToken>()).Returns(service);
        _scheduleRepository.GetByProfessionalAndDayAsync(professionalId, DayOfWeek.Monday, Arg.Any<CancellationToken>()).Returns(schedule);
        _appointmentRepository.GetByProfessionalAndDateAsync(professionalId, date, Arg.Any<CancellationToken>())
            .Returns(new List<Appointment>());
        _absenceRepository.GetByProfessionalAndDateAsync(professionalId, date, Arg.Any<CancellationToken>())
            .Returns(new List<Absence>());

        var query = new GetAvailabilityQuery(professionalId, date, service.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Slots.Should().HaveCount(8); // 9:00, 9:30, 10:00, 10:30, 11:00, 11:30, 12:00, 12:30
    }

    [Fact]
    public async Task Handle_WithFullDayAbsence_ShouldReturnEmptySlots()
    {
        var professionalId = Guid.NewGuid();
        var service = Service.Create(_tenantId, "Haircut", TimeSpan.FromMinutes(30), 50m).Value;
        var date = GetNextMonday();

        var schedule = Schedule.Create(_tenantId, professionalId, DayOfWeek.Monday,
            new TimeOnly(9, 0), new TimeOnly(13, 0), TimeSpan.FromMinutes(30)).Value;

        var fullDayAbsence = Absence.Create(_tenantId, professionalId, date, null, null, "Vacation").Value;

        _serviceRepository.GetByIdAsync(service.Id, Arg.Any<CancellationToken>()).Returns(service);
        _scheduleRepository.GetByProfessionalAndDayAsync(professionalId, DayOfWeek.Monday, Arg.Any<CancellationToken>()).Returns(schedule);
        _absenceRepository.GetByProfessionalAndDateAsync(professionalId, date, Arg.Any<CancellationToken>())
            .Returns(new List<Absence> { fullDayAbsence });

        var query = new GetAvailabilityQuery(professionalId, date, service.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Slots.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithPartialAbsence_ShouldBlockOnlyThoseSlots()
    {
        var professionalId = Guid.NewGuid();
        var service = Service.Create(_tenantId, "Haircut", TimeSpan.FromMinutes(30), 50m).Value;
        var date = GetNextMonday();

        var schedule = Schedule.Create(_tenantId, professionalId, DayOfWeek.Monday,
            new TimeOnly(9, 0), new TimeOnly(13, 0), TimeSpan.FromMinutes(30)).Value;

        // Absence from 9:00 to 11:00 should block slots at 9:00, 9:30, 10:00, 10:30
        var partialAbsence = Absence.Create(_tenantId, professionalId, date,
            new TimeOnly(9, 0), new TimeOnly(11, 0), "Morning appointment").Value;

        _serviceRepository.GetByIdAsync(service.Id, Arg.Any<CancellationToken>()).Returns(service);
        _scheduleRepository.GetByProfessionalAndDayAsync(professionalId, DayOfWeek.Monday, Arg.Any<CancellationToken>()).Returns(schedule);
        _appointmentRepository.GetByProfessionalAndDateAsync(professionalId, date, Arg.Any<CancellationToken>())
            .Returns(new List<Appointment>());
        _absenceRepository.GetByProfessionalAndDateAsync(professionalId, date, Arg.Any<CancellationToken>())
            .Returns(new List<Absence> { partialAbsence });

        var query = new GetAvailabilityQuery(professionalId, date, service.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Slots.Should().HaveCount(4); // 11:00, 11:30, 12:00, 12:30
        result.Value.Slots.Should().NotContain(s => s.Start < new TimeOnly(11, 0));
        result.Value.Slots.Should().OnlyContain(s => s.Start >= new TimeOnly(11, 0));
    }
}
