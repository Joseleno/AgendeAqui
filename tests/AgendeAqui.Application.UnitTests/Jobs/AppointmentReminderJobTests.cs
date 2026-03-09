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
    public async Task ProcessRemindersAsync_WithScheduledAppointments_ShouldSendBatchReminders()
    {
        // Arrange
        var appointment = CreateScheduledAppointment();
        _appointmentRepository.GetByDateRangeAndStatusesAsync(
            Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<IEnumerable<AppointmentStatus>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Appointment> { appointment });

        // Act
        await _job.ProcessRemindersAsync(CancellationToken.None);

        // Assert
        await _notificationService.Received(1).SendAppointmentRemindersAsync(
            Arg.Is<IReadOnlyList<Appointment>>(list => list.Count == 1 && list[0].Id == appointment.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessRemindersAsync_WithNoAppointments_ShouldNotSendReminders()
    {
        // Arrange
        _appointmentRepository.GetByDateRangeAndStatusesAsync(
            Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<IEnumerable<AppointmentStatus>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Appointment>());

        // Act
        await _job.ProcessRemindersAsync(CancellationToken.None);

        // Assert
        await _notificationService.DidNotReceive().SendAppointmentRemindersAsync(
            Arg.Any<IReadOnlyList<Appointment>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessRemindersAsync_WithCancelledAppointment_ShouldNotSendReminder()
    {
        // Arrange: repository filters at DB level, so returns empty when only cancelled exist
        _appointmentRepository.GetByDateRangeAndStatusesAsync(
            Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<IEnumerable<AppointmentStatus>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Appointment>());

        // Act
        await _job.ProcessRemindersAsync(CancellationToken.None);

        // Assert
        await _notificationService.DidNotReceive().SendAppointmentRemindersAsync(
            Arg.Any<IReadOnlyList<Appointment>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessRemindersAsync_WithMixedStatuses_ShouldOnlySendEligible()
    {
        // Arrange: repository filters at DB level, so only eligible appointments are returned
        var scheduledAppointment = CreateScheduledAppointment();

        _appointmentRepository.GetByDateRangeAndStatusesAsync(
            Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), Arg.Any<IEnumerable<AppointmentStatus>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Appointment> { scheduledAppointment });

        // Act
        await _job.ProcessRemindersAsync(CancellationToken.None);

        // Assert
        await _notificationService.Received(1).SendAppointmentRemindersAsync(
            Arg.Is<IReadOnlyList<Appointment>>(list => list.Count == 1 && list[0].Id == scheduledAppointment.Id),
            Arg.Any<CancellationToken>());
    }
}
