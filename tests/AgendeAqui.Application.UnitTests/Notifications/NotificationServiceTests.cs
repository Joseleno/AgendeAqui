using AgendeAqui.Application.Abstractions.Notifications;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments;
using AgendeAqui.Domain.Clients;
using AgendeAqui.Domain.Notifications;
using AgendeAqui.Domain.ValueObjects;
using AgendeAqui.Infrastructure.Notifications;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Notifications;

public class NotificationServiceTests
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IWhatsAppClient _whatsAppClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly NotificationService _service;

    public NotificationServiceTests()
    {
        _notificationRepository = Substitute.For<INotificationRepository>();
        _appointmentRepository = Substitute.For<IAppointmentRepository>();
        _clientRepository = Substitute.For<IClientRepository>();
        _whatsAppClient = Substitute.For<IWhatsAppClient>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        var logger = Substitute.For<ILogger<NotificationService>>();

        _service = new NotificationService(
            _notificationRepository,
            _appointmentRepository,
            _clientRepository,
            _whatsAppClient,
            _unitOfWork,
            logger);
    }

    private static Appointment CreateAppointment(Guid? tenantId = null)
    {
        return Appointment.Create(
            tenantId ?? Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            TimeSlot.Create(new TimeOnly(9, 0), new TimeOnly(10, 0)).Value).Value;
    }

    private static Client CreateClient(Guid? tenantId = null)
    {
        return Client.Create(
            tenantId ?? Guid.NewGuid(),
            "Maria Silva",
            Email.Create("maria@test.com").Value,
            PhoneNumber.Create("5521912345678").Value).Value;
    }

    [Fact]
    public async Task SendAppointmentCreatedAsync_WithValidData_ShouldSendAndSave()
    {
        var appointment = CreateAppointment();
        var client = CreateClient();

        _appointmentRepository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);
        _clientRepository.GetByIdAsync(appointment.ClientId, Arg.Any<CancellationToken>()).Returns(client);
        _whatsAppClient.SendTemplateMessageAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        await _service.SendAppointmentCreatedAsync(appointment.TenantId, appointment.Id, CancellationToken.None);

        await _whatsAppClient.Received(1).SendTemplateMessageAsync(
            Arg.Any<string>(), "appointment_created", Arg.Any<Dictionary<string, string>>(), Arg.Any<CancellationToken>());
        await _notificationRepository.Received(1).AddAsync(Arg.Any<Domain.Notifications.Notification>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAppointmentCreatedAsync_WithNonExistentAppointment_ShouldNotSend()
    {
        _appointmentRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Appointment?)null);

        await _service.SendAppointmentCreatedAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        await _whatsAppClient.DidNotReceive().SendTemplateMessageAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<CancellationToken>());
        await _notificationRepository.DidNotReceive().AddAsync(Arg.Any<Domain.Notifications.Notification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAppointmentCreatedAsync_WithNonExistentClient_ShouldNotSend()
    {
        var appointment = CreateAppointment();

        _appointmentRepository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);
        _clientRepository.GetByIdAsync(appointment.ClientId, Arg.Any<CancellationToken>()).Returns((Client?)null);

        await _service.SendAppointmentCreatedAsync(appointment.TenantId, appointment.Id, CancellationToken.None);

        await _whatsAppClient.DidNotReceive().SendTemplateMessageAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<CancellationToken>());
        await _notificationRepository.DidNotReceive().AddAsync(Arg.Any<Domain.Notifications.Notification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAppointmentCreatedAsync_WhenWhatsAppFails_ShouldSaveFailedNotification()
    {
        var appointment = CreateAppointment();
        var client = CreateClient();

        _appointmentRepository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);
        _clientRepository.GetByIdAsync(appointment.ClientId, Arg.Any<CancellationToken>()).Returns(client);
        _whatsAppClient.SendTemplateMessageAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        await _service.SendAppointmentCreatedAsync(appointment.TenantId, appointment.Id, CancellationToken.None);

        await _notificationRepository.Received(1).AddAsync(
            Arg.Is<Domain.Notifications.Notification>(n => n.Status == NotificationStatus.Failed),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAppointmentCreatedAsync_WhenWhatsAppThrows_ShouldSaveFailedNotification()
    {
        var appointment = CreateAppointment();
        var client = CreateClient();

        _appointmentRepository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);
        _clientRepository.GetByIdAsync(appointment.ClientId, Arg.Any<CancellationToken>()).Returns(client);
        _whatsAppClient.SendTemplateMessageAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns<bool>(_ => { throw new HttpRequestException("Connection refused"); });
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        await _service.SendAppointmentCreatedAsync(appointment.TenantId, appointment.Id, CancellationToken.None);

        await _notificationRepository.Received(1).AddAsync(
            Arg.Is<Domain.Notifications.Notification>(n => n.Status == NotificationStatus.Failed),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAppointmentCancelledAsync_WithValidData_ShouldSendWithReasonParameter()
    {
        var appointment = CreateAppointment();
        var client = CreateClient();
        const string reason = "Customer request";

        _appointmentRepository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);
        _clientRepository.GetByIdAsync(appointment.ClientId, Arg.Any<CancellationToken>()).Returns(client);
        _whatsAppClient.SendTemplateMessageAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        await _service.SendAppointmentCancelledAsync(appointment.TenantId, appointment.Id, reason, CancellationToken.None);

        await _whatsAppClient.Received(1).SendTemplateMessageAsync(
            Arg.Any<string>(),
            "appointment_cancelled",
            Arg.Is<Dictionary<string, string>>(p => p["reason"] == reason),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAppointmentRescheduledAsync_WithValidData_ShouldSendWithNewDateParameter()
    {
        var appointment = CreateAppointment();
        var client = CreateClient();
        var newDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

        _appointmentRepository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);
        _clientRepository.GetByIdAsync(appointment.ClientId, Arg.Any<CancellationToken>()).Returns(client);
        _whatsAppClient.SendTemplateMessageAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        await _service.SendAppointmentRescheduledAsync(appointment.TenantId, appointment.Id, newDate, CancellationToken.None);

        await _whatsAppClient.Received(1).SendTemplateMessageAsync(
            Arg.Any<string>(),
            "appointment_rescheduled",
            Arg.Is<Dictionary<string, string>>(p => p["new_date"] == newDate.ToString("dd/MM/yyyy")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAppointmentReminderAsync_WithValidData_ShouldSendWithReminderTemplate()
    {
        var appointment = CreateAppointment();
        var client = CreateClient();

        _appointmentRepository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);
        _clientRepository.GetByIdAsync(appointment.ClientId, Arg.Any<CancellationToken>()).Returns(client);
        _whatsAppClient.SendTemplateMessageAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        await _service.SendAppointmentReminderAsync(appointment.TenantId, appointment.Id, CancellationToken.None);

        await _whatsAppClient.Received(1).SendTemplateMessageAsync(
            Arg.Any<string>(), "appointment_reminder", Arg.Any<Dictionary<string, string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAppointmentCreatedAsync_WithSentNotification_ShouldSetCorrectPhoneNumber()
    {
        var tenantId = Guid.NewGuid();
        var appointment = CreateAppointment(tenantId);
        var client = CreateClient(tenantId);

        _appointmentRepository.GetByIdAsync(appointment.Id, Arg.Any<CancellationToken>()).Returns(appointment);
        _clientRepository.GetByIdAsync(appointment.ClientId, Arg.Any<CancellationToken>()).Returns(client);
        _whatsAppClient.SendTemplateMessageAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        await _service.SendAppointmentCreatedAsync(tenantId, appointment.Id, CancellationToken.None);

        await _whatsAppClient.Received(1).SendTemplateMessageAsync(
            client.Phone.Value, Arg.Any<string>(), Arg.Any<Dictionary<string, string>>(), Arg.Any<CancellationToken>());
        await _notificationRepository.Received(1).AddAsync(
            Arg.Is<Domain.Notifications.Notification>(n => n.Status == NotificationStatus.Sent && n.Recipient == client.Phone.Value),
            Arg.Any<CancellationToken>());
    }
}
