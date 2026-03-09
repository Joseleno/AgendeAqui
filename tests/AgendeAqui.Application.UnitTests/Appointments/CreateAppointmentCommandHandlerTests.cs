using AgendeAqui.Application.Appointments.CreateAppointment;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments;
using AgendeAqui.Domain.Clients;
using AgendeAqui.Domain.Professionals;
using AgendeAqui.Domain.Schedules;
using AgendeAqui.Domain.Services;
using AgendeAqui.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Appointments;

public class CreateAppointmentCommandHandlerTests
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IProfessionalRepository _professionalRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUser _currentUser;
    private readonly CreateAppointmentCommandHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public CreateAppointmentCommandHandlerTests()
    {
        _appointmentRepository = Substitute.For<IAppointmentRepository>();
        _professionalRepository = Substitute.For<IProfessionalRepository>();
        _serviceRepository = Substitute.For<IServiceRepository>();
        _clientRepository = Substitute.For<IClientRepository>();
        _scheduleRepository = Substitute.For<IScheduleRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _tenantProvider.GetTenantId().Returns(_tenantId);
        _currentUser = Substitute.For<ICurrentUser>();
        _currentUser.Role.Returns("Admin");

        _handler = new CreateAppointmentCommandHandler(
            _appointmentRepository,
            _professionalRepository,
            _serviceRepository,
            _clientRepository,
            _scheduleRepository,
            _unitOfWork,
            _tenantProvider,
            _currentUser);
    }

    private Professional CreateProfessional() =>
        Professional.Create(_tenantId, "John", Email.Create("john@test.com").Value, PhoneNumber.Create("5511987654321").Value).Value;

    private Service CreateService(int durationMinutes = 60) =>
        Service.Create(_tenantId, "Haircut", TimeSpan.FromMinutes(durationMinutes), 50m).Value;

    private Client CreateClient() =>
        Client.Create(_tenantId, "Maria", Email.Create("maria@test.com").Value, PhoneNumber.Create("5521912345678").Value).Value;

    private Schedule CreateSchedule(Guid professionalId, DayOfWeek day = DayOfWeek.Monday) =>
        Schedule.Create(_tenantId, professionalId, day, new TimeOnly(8, 0), new TimeOnly(18, 0), TimeSpan.FromMinutes(30)).Value;

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateAppointment()
    {
        var professional = CreateProfessional();
        var service = CreateService();
        var client = CreateClient();
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        while (date.DayOfWeek != DayOfWeek.Monday) date = date.AddDays(1);
        var schedule = CreateSchedule(professional.Id, DayOfWeek.Monday);

        _professionalRepository.GetByIdAsync(professional.Id, Arg.Any<CancellationToken>()).Returns(professional);
        _serviceRepository.GetByIdAsync(service.Id, Arg.Any<CancellationToken>()).Returns(service);
        _clientRepository.GetByIdAsync(client.Id, Arg.Any<CancellationToken>()).Returns(client);
        _scheduleRepository.GetByProfessionalAndDayAsync(professional.Id, DayOfWeek.Monday, Arg.Any<CancellationToken>()).Returns(schedule);
        _appointmentRepository.HasConflictAsync(Arg.Any<Guid>(), Arg.Any<DateOnly>(), Arg.Any<TimeOnly>(), Arg.Any<TimeOnly>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns(false);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = new CreateAppointmentCommand(professional.Id, service.Id, client.Id, date, new TimeOnly(9, 0), null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _appointmentRepository.Received(1).AddAsync(Arg.Any<Appointment>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentProfessional_ShouldReturnFailure()
    {
        _professionalRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Professional?)null);

        var command = new CreateAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today.AddDays(1)), new TimeOnly(9, 0), null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AppointmentErrors.ProfessionalNotFound);
    }

    [Fact]
    public async Task Handle_WithInactiveProfessional_ShouldReturnFailure()
    {
        var professional = CreateProfessional();
        professional.Deactivate();
        _professionalRepository.GetByIdAsync(professional.Id, Arg.Any<CancellationToken>()).Returns(professional);

        var command = new CreateAppointmentCommand(professional.Id, Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today.AddDays(1)), new TimeOnly(9, 0), null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AppointmentErrors.ProfessionalInactive);
    }

    [Fact]
    public async Task Handle_WithNonExistentService_ShouldReturnFailure()
    {
        var professional = CreateProfessional();
        _professionalRepository.GetByIdAsync(professional.Id, Arg.Any<CancellationToken>()).Returns(professional);
        _serviceRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Service?)null);

        var command = new CreateAppointmentCommand(professional.Id, Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today.AddDays(1)), new TimeOnly(9, 0), null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AppointmentErrors.ServiceNotFound);
    }

    [Fact]
    public async Task Handle_WithNonExistentClient_ShouldReturnFailure()
    {
        var professional = CreateProfessional();
        var service = CreateService();
        _professionalRepository.GetByIdAsync(professional.Id, Arg.Any<CancellationToken>()).Returns(professional);
        _serviceRepository.GetByIdAsync(service.Id, Arg.Any<CancellationToken>()).Returns(service);
        _clientRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Client?)null);

        var command = new CreateAppointmentCommand(professional.Id, service.Id, Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today.AddDays(1)), new TimeOnly(9, 0), null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AppointmentErrors.ClientNotFound);
    }

    [Fact]
    public async Task Handle_WithNoSchedule_ShouldReturnFailure()
    {
        var professional = CreateProfessional();
        var service = CreateService();
        var client = CreateClient();
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

        _professionalRepository.GetByIdAsync(professional.Id, Arg.Any<CancellationToken>()).Returns(professional);
        _serviceRepository.GetByIdAsync(service.Id, Arg.Any<CancellationToken>()).Returns(service);
        _clientRepository.GetByIdAsync(client.Id, Arg.Any<CancellationToken>()).Returns(client);
        _scheduleRepository.GetByProfessionalAndDayAsync(professional.Id, Arg.Any<DayOfWeek>(), Arg.Any<CancellationToken>()).Returns((Schedule?)null);

        var command = new CreateAppointmentCommand(professional.Id, service.Id, client.Id, date, new TimeOnly(9, 0), null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AppointmentErrors.NoSchedule);
    }

    [Fact]
    public async Task Handle_WithConflict_ShouldReturnFailure()
    {
        var professional = CreateProfessional();
        var service = CreateService();
        var client = CreateClient();
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        while (date.DayOfWeek != DayOfWeek.Monday) date = date.AddDays(1);
        var schedule = CreateSchedule(professional.Id, DayOfWeek.Monday);

        _professionalRepository.GetByIdAsync(professional.Id, Arg.Any<CancellationToken>()).Returns(professional);
        _serviceRepository.GetByIdAsync(service.Id, Arg.Any<CancellationToken>()).Returns(service);
        _clientRepository.GetByIdAsync(client.Id, Arg.Any<CancellationToken>()).Returns(client);
        _scheduleRepository.GetByProfessionalAndDayAsync(professional.Id, DayOfWeek.Monday, Arg.Any<CancellationToken>()).Returns(schedule);
        _appointmentRepository.HasConflictAsync(Arg.Any<Guid>(), Arg.Any<DateOnly>(), Arg.Any<TimeOnly>(), Arg.Any<TimeOnly>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns(true);

        var command = new CreateAppointmentCommand(professional.Id, service.Id, client.Id, date, new TimeOnly(9, 0), null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AppointmentErrors.Conflict);
    }

    [Fact]
    public async Task Handle_WithInactiveService_ShouldReturnFailure()
    {
        var professional = CreateProfessional();
        var service = CreateService();
        service.Deactivate();
        _professionalRepository.GetByIdAsync(professional.Id, Arg.Any<CancellationToken>()).Returns(professional);
        _serviceRepository.GetByIdAsync(service.Id, Arg.Any<CancellationToken>()).Returns(service);

        var command = new CreateAppointmentCommand(professional.Id, service.Id, Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today.AddDays(1)), new TimeOnly(9, 0), null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AppointmentErrors.ServiceInactive);
    }

    [Fact]
    public async Task Handle_OutsideSchedule_ShouldReturnFailure()
    {
        var professional = CreateProfessional();
        var service = CreateService();
        var client = CreateClient();
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        while (date.DayOfWeek != DayOfWeek.Monday) date = date.AddDays(1);
        var schedule = CreateSchedule(professional.Id, DayOfWeek.Monday);

        _professionalRepository.GetByIdAsync(professional.Id, Arg.Any<CancellationToken>()).Returns(professional);
        _serviceRepository.GetByIdAsync(service.Id, Arg.Any<CancellationToken>()).Returns(service);
        _clientRepository.GetByIdAsync(client.Id, Arg.Any<CancellationToken>()).Returns(client);
        _scheduleRepository.GetByProfessionalAndDayAsync(professional.Id, DayOfWeek.Monday, Arg.Any<CancellationToken>()).Returns(schedule);

        var command = new CreateAppointmentCommand(professional.Id, service.Id, client.Id, date, new TimeOnly(6, 0), null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AppointmentErrors.OutsideSchedule);
    }
}
