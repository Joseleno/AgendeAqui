using AgendeAqui.Domain.Notifications;
using FluentAssertions;

namespace AgendeAqui.Domain.UnitTests.Notifications;

public class NotificationTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var result = Notification.Create(
            Guid.NewGuid(), Guid.NewGuid(), NotificationChannel.WhatsApp, "+5511999999999", "appointment_created");

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(NotificationStatus.Pending);
        result.Value.Channel.Should().Be(NotificationChannel.WhatsApp);
    }

    [Fact]
    public void Create_WithEmptyTenantId_ShouldFail()
    {
        var result = Notification.Create(
            Guid.Empty, Guid.NewGuid(), NotificationChannel.WhatsApp, "+5511999999999", "template");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(NotificationErrors.InvalidTenant);
    }

    [Fact]
    public void Create_WithEmptyAppointmentId_ShouldFail()
    {
        var result = Notification.Create(
            Guid.NewGuid(), Guid.Empty, NotificationChannel.WhatsApp, "+5511999999999", "template");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(NotificationErrors.InvalidAppointment);
    }

    [Fact]
    public void Create_WithEmptyRecipient_ShouldFail()
    {
        var result = Notification.Create(
            Guid.NewGuid(), Guid.NewGuid(), NotificationChannel.WhatsApp, "", "template");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(NotificationErrors.InvalidRecipient);
    }

    [Fact]
    public void Create_WithWhitespaceRecipient_ShouldFail()
    {
        var result = Notification.Create(
            Guid.NewGuid(), Guid.NewGuid(), NotificationChannel.WhatsApp, "   ", "template");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(NotificationErrors.InvalidRecipient);
    }

    [Fact]
    public void Create_WithEmptyTemplateName_ShouldFail()
    {
        var result = Notification.Create(
            Guid.NewGuid(), Guid.NewGuid(), NotificationChannel.WhatsApp, "+5511999999999", "");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(NotificationErrors.InvalidTemplateName);
    }

    [Fact]
    public void MarkAsSent_ShouldUpdateStatusAndSentAt()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var notification = Notification.Create(
            Guid.NewGuid(), Guid.NewGuid(), NotificationChannel.WhatsApp, "+5511999999999", "template").Value;

        notification.MarkAsSent();

        notification.Status.Should().Be(NotificationStatus.Sent);
        notification.SentAt.Should().NotBeNull();
        notification.SentAt.Should().BeAfter(before);
        notification.UpdatedAt.Should().BeAfter(before);
    }

    [Fact]
    public void MarkAsFailed_ShouldUpdateStatusAndErrorMessage()
    {
        var notification = Notification.Create(
            Guid.NewGuid(), Guid.NewGuid(), NotificationChannel.WhatsApp, "+5511999999999", "template").Value;

        notification.MarkAsFailed("API timeout");

        notification.Status.Should().Be(NotificationStatus.Failed);
        notification.ErrorMessage.Should().Be("API timeout");
        notification.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsDelivered_ShouldUpdateStatus()
    {
        var notification = Notification.Create(
            Guid.NewGuid(), Guid.NewGuid(), NotificationChannel.WhatsApp, "+5511999999999", "template").Value;
        notification.MarkAsSent();

        notification.MarkAsDelivered();

        notification.Status.Should().Be(NotificationStatus.Delivered);
        notification.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var tenantId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();

        var result = Notification.Create(tenantId, appointmentId, NotificationChannel.Sms, "+5511999999999", "reminder");

        result.IsSuccess.Should().BeTrue();
        var n = result.Value;
        n.TenantId.Should().Be(tenantId);
        n.AppointmentId.Should().Be(appointmentId);
        n.Channel.Should().Be(NotificationChannel.Sms);
        n.Recipient.Should().Be("+5511999999999");
        n.TemplateName.Should().Be("reminder");
        n.Status.Should().Be(NotificationStatus.Pending);
        n.SentAt.Should().BeNull();
        n.ErrorMessage.Should().BeNull();
    }
}
