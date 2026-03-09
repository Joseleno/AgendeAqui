using AgendeAqui.Application.Abstractions.Notifications;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments;
using AgendeAqui.Domain.ValueObjects;
using AgendeAqui.Infrastructure.Jobs;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AgendeAqui.Application.UnitTests.Jobs;

public class AppointmentReminderJobTests
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly INotificationService _notificationService;
    private readonly AppointmentReminderJob _job;

    public AppointmentReminderJobTests()
    {
        _appointmentRepository = Substitute.For<IAppointmentRepository>();
        _notificationService = Substitute.For<INotificationService>();

        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IAppointmentRepository)).Returns(_appointmentRepository);
        serviceProvider.GetService(typeof(INotificationService)).Returns(_notificationService);

        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(serviceProvider);

        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        scopeFactory.CreateScope().Returns(scope);

        var logger = Substitute.For<ILogger<AppointmentReminderJob>>();
        var settings = Options.Create(new ReminderSettings
        {
            IntervalMinutes = 60,
            HoursBeforeAppointment = 24,
            TimezoneOffsetHours = -3
        });

        _job = new AppointmentReminderJob(scopeFactory, logger, settings);
    }

    private static Appointment CreateScheduledAppointment()
    {
        var timeSlot = TimeSlot.Create(new TimeOnly(9, 0), new TimeOnly(10, 0)).Value;
        return Appointment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            timeSlot).Value;
    }

    [Fact]
    public async Task ProcessRemindersAsync_WithScheduledAppointments_ShouldSendReminders()
    {
        // Arrange
        var appointment = CreateScheduledAppointment();
        _appointmentRepository.GetByDateRangeAsync(
            Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(new List<Appointment> { appointment });

        // Act
        await _job.ProcessRemindersAsync(CancellationToken.None);

        // Assert
        await _notificationService.Received(1).SendAppointmentReminderAsync(
            appointment.TenantId, appointment.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessRemindersAsync_WithNoAppointments_ShouldNotSendReminders()
    {
        // Arrange
        _appointmentRepository.GetByDateRangeAsync(
            Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(new List<Appointment>());

        // Act
        await _job.ProcessRemindersAsync(CancellationToken.None);

        // Assert
        await _notificationService.DidNotReceive().SendAppointmentReminderAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessRemindersAsync_WithCancelledAppointment_ShouldNotSendReminder()
    {
        // Arrange
        var appointment = CreateScheduledAppointment();
        appointment.Cancel("test reason");

        _appointmentRepository.GetByDateRangeAsync(
            Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(new List<Appointment> { appointment });

        // Act
        await _job.ProcessRemindersAsync(CancellationToken.None);

        // Assert
        await _notificationService.DidNotReceive().SendAppointmentReminderAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessRemindersAsync_WhenReminderFails_ShouldContinueWithOthers()
    {
        // Arrange
        var appointment1 = CreateScheduledAppointment();
        var appointment2 = CreateScheduledAppointment();

        _appointmentRepository.GetByDateRangeAsync(
            Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(new List<Appointment> { appointment1, appointment2 });

        _notificationService.SendAppointmentReminderAsync(
            appointment1.TenantId, appointment1.Id, Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("WhatsApp down"));

        // Act
        await _job.ProcessRemindersAsync(CancellationToken.None);

        // Assert
        await _notificationService.Received(1).SendAppointmentReminderAsync(
            appointment2.TenantId, appointment2.Id, Arg.Any<CancellationToken>());
    }
}
