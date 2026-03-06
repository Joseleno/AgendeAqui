using AgendeAqui.Domain.Appointments;
using AgendeAqui.Domain.Appointments.Events;
using AgendeAqui.Domain.ValueObjects;
using FluentAssertions;

namespace AgendeAqui.Domain.UnitTests.Appointments;

public class AppointmentTests
{
    private static TimeSlot CreateTimeSlot(int startHour = 9, int endHour = 10) =>
        TimeSlot.Create(new TimeOnly(startHour, 0), new TimeOnly(endHour, 0)).Value;

    private static Appointment CreateScheduledAppointment(TimeSlot? timeSlot = null) =>
        Appointment.Create(
            tenantId: Guid.NewGuid(),
            professionalId: Guid.NewGuid(),
            serviceId: Guid.NewGuid(),
            clientId: Guid.NewGuid(),
            date: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            timeSlot: timeSlot ?? CreateTimeSlot()).Value;

    [Fact]
    public void Create_WithEmptyTenantId_ShouldFail()
    {
        var result = Appointment.Create(
            Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)), CreateTimeSlot());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AppointmentErrors.InvalidTenant);
    }

    [Fact]
    public void Create_WithEmptyProfessionalId_ShouldFail()
    {
        var result = Appointment.Create(
            Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)), CreateTimeSlot());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AppointmentErrors.ProfessionalNotFound);
    }

    [Fact]
    public void Create_WithEmptyServiceId_ShouldFail()
    {
        var result = Appointment.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)), CreateTimeSlot());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AppointmentErrors.ServiceNotFound);
    }

    [Fact]
    public void Create_WithEmptyClientId_ShouldFail()
    {
        var result = Appointment.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.Empty,
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)), CreateTimeSlot());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AppointmentErrors.ClientNotFound);
    }

    [Fact]
    public void Create_ShouldRaiseAppointmentCreatedEvent()
    {
        var appointment = CreateScheduledAppointment();

        appointment.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AppointmentCreatedEvent>();
    }

    [Fact]
    public void Create_ShouldSetStatusToScheduled()
    {
        var appointment = CreateScheduledAppointment();

        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    [Fact]
    public void Create_ShouldPopulateAllProperties()
    {
        var tenantId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var timeSlot = CreateTimeSlot();
        var notes = "Patient notes";

        var result = Appointment.Create(tenantId, professionalId, serviceId, clientId, date, timeSlot, notes);

        result.IsSuccess.Should().BeTrue();
        var appointment = result.Value;
        appointment.TenantId.Should().Be(tenantId);
        appointment.ProfessionalId.Should().Be(professionalId);
        appointment.ServiceId.Should().Be(serviceId);
        appointment.ClientId.Should().Be(clientId);
        appointment.Date.Should().Be(date);
        appointment.TimeSlot.Should().Be(timeSlot);
        appointment.Notes.Should().Be(notes);
    }

    [Fact]
    public void Confirm_FromScheduled_ShouldSucceed()
    {
        var appointment = CreateScheduledAppointment();

        var result = appointment.Confirm();

        result.IsSuccess.Should().BeTrue();
        appointment.Status.Should().Be(AppointmentStatus.Confirmed);
    }

    [Fact]
    public void Confirm_FromScheduled_ShouldRaiseConfirmedEvent()
    {
        var appointment = CreateScheduledAppointment();
        appointment.ClearDomainEvents();

        appointment.Confirm();

        appointment.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AppointmentConfirmedEvent>();
    }

    [Fact]
    public void Cancel_FromConfirmed_ShouldSucceed()
    {
        var appointment = CreateScheduledAppointment();
        appointment.Confirm();

        var result = appointment.Cancel("Changed mind");

        result.IsSuccess.Should().BeTrue();
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
    }

    [Fact]
    public void Cancel_FromScheduled_ShouldSucceed()
    {
        var appointment = CreateScheduledAppointment();

        var result = appointment.Cancel("Patient request");

        result.IsSuccess.Should().BeTrue();
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
    }

    [Fact]
    public void Cancel_FromScheduled_ShouldRaiseCancelledEvent()
    {
        var appointment = CreateScheduledAppointment();
        appointment.ClearDomainEvents();

        appointment.Cancel("No longer needed");

        appointment.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AppointmentCancelledEvent>();
    }

    [Fact]
    public void Cancel_FromNoShow_ShouldFail()
    {
        var appointment = CreateScheduledAppointment();
        appointment.Confirm();
        appointment.Start();
        appointment.MarkNoShow();

        var result = appointment.Cancel("Too late");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AppointmentErrors.InvalidTransition);
    }

    [Fact]
    public void Complete_FromInProgress_ShouldRaiseCompletedEvent()
    {
        var appointment = CreateScheduledAppointment();
        appointment.Confirm();
        appointment.Start();
        appointment.ClearDomainEvents();

        appointment.Complete();

        appointment.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AppointmentCompletedEvent>();
    }

    [Fact]
    public void MarkNoShow_FromInProgress_ShouldRaiseNoShowEvent()
    {
        var appointment = CreateScheduledAppointment();
        appointment.Confirm();
        appointment.Start();
        appointment.ClearDomainEvents();

        appointment.MarkNoShow();

        appointment.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AppointmentNoShowEvent>();
    }

    [Fact]
    public void Reschedule_FromInProgress_ShouldFail()
    {
        var appointment = CreateScheduledAppointment();
        appointment.Confirm();
        appointment.Start();

        var result = appointment.Reschedule(
            DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
            CreateTimeSlot(14, 15));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AppointmentErrors.InvalidTransition);
    }

    [Fact]
    public void Reschedule_FromCompleted_ShouldFail()
    {
        var appointment = CreateScheduledAppointment();
        appointment.Confirm();
        appointment.Start();
        appointment.Complete();

        var result = appointment.Reschedule(
            DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
            CreateTimeSlot(14, 15));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AppointmentErrors.InvalidTransition);
    }

    [Fact]
    public void Reschedule_FromScheduled_ShouldSucceed()
    {
        var appointment = CreateScheduledAppointment();
        var newDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
        var newSlot = CreateTimeSlot(14, 15);

        var result = appointment.Reschedule(newDate, newSlot);

        result.IsSuccess.Should().BeTrue();
        appointment.Date.Should().Be(newDate);
        appointment.TimeSlot.Should().Be(newSlot);
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    [Fact]
    public void Reschedule_FromConfirmed_ShouldSucceed()
    {
        var appointment = CreateScheduledAppointment();
        appointment.Confirm();
        var newDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
        var newSlot = CreateTimeSlot(14, 15);

        var result = appointment.Reschedule(newDate, newSlot);

        result.IsSuccess.Should().BeTrue();
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    [Fact]
    public void Reschedule_FromCancelled_ShouldFail()
    {
        var appointment = CreateScheduledAppointment();
        appointment.Cancel("Patient request");

        var result = appointment.Reschedule(
            DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
            CreateTimeSlot(14, 15));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AppointmentErrors.InvalidTransition);
    }

    [Fact]
    public void Reschedule_ShouldRaiseRescheduledEvent()
    {
        var appointment = CreateScheduledAppointment();
        appointment.ClearDomainEvents();
        var newDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5));

        appointment.Reschedule(newDate, CreateTimeSlot(14, 15));

        appointment.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AppointmentRescheduledEvent>();
    }

    [Fact]
    public void MarkNoShow_FromInProgress_ShouldSucceed()
    {
        var appointment = CreateScheduledAppointment();
        appointment.Confirm();
        appointment.Start();

        var result = appointment.MarkNoShow();

        result.IsSuccess.Should().BeTrue();
        appointment.Status.Should().Be(AppointmentStatus.NoShow);
    }

    [Fact]
    public void MarkNoShow_FromScheduled_ShouldFail()
    {
        var appointment = CreateScheduledAppointment();

        var result = appointment.MarkNoShow();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AppointmentErrors.InvalidTransition);
    }

    [Fact]
    public void Complete_FromInProgress_ShouldSucceed()
    {
        var appointment = CreateScheduledAppointment();
        appointment.Confirm();
        appointment.Start();

        var result = appointment.Complete();

        result.IsSuccess.Should().BeTrue();
        appointment.Status.Should().Be(AppointmentStatus.Completed);
    }

    [Fact]
    public void Complete_FromConfirmed_ShouldFail()
    {
        var appointment = CreateScheduledAppointment();
        appointment.Confirm();

        var result = appointment.Complete();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AppointmentErrors.InvalidTransition);
    }

    [Fact]
    public void Start_FromConfirmed_ShouldSucceed()
    {
        var appointment = CreateScheduledAppointment();
        appointment.Confirm();

        var result = appointment.Start();

        result.IsSuccess.Should().BeTrue();
        appointment.Status.Should().Be(AppointmentStatus.InProgress);
    }

    [Fact]
    public void Start_FromConfirmed_ShouldRaiseStartedEvent()
    {
        var appointment = CreateScheduledAppointment();
        appointment.Confirm();
        appointment.ClearDomainEvents();

        appointment.Start();

        appointment.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AppointmentStartedEvent>();
    }

    [Fact]
    public void Start_FromScheduled_ShouldFail()
    {
        var appointment = CreateScheduledAppointment();

        var result = appointment.Start();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AppointmentErrors.InvalidTransition);
    }
}
