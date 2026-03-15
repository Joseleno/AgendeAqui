using AgendeAqui.Domain.InAppNotifications;
using FluentAssertions;

namespace AgendeAqui.Domain.UnitTests.InAppNotifications;

public sealed class InAppNotificationTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldCreateNotification()
    {
        var referenceId = Guid.NewGuid();

        var notification = InAppNotification.Create(
            _tenantId,
            _userId,
            "New appointment",
            "A new appointment has been scheduled.",
            InAppNotificationType.AppointmentCreated,
            referenceId);

        notification.TenantId.Should().Be(_tenantId);
        notification.UserId.Should().Be(_userId);
        notification.Title.Should().Be("New appointment");
        notification.Message.Should().Be("A new appointment has been scheduled.");
        notification.Type.Should().Be(InAppNotificationType.AppointmentCreated);
        notification.ReferenceId.Should().Be(referenceId);
        notification.IsRead.Should().BeFalse();
        notification.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_WithNullTitle_ShouldThrow()
    {
        var act = () => InAppNotification.Create(
            _tenantId,
            _userId,
            null!,
            "Some message",
            InAppNotificationType.AppointmentCreated);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNullMessage_ShouldThrow()
    {
        var act = () => InAppNotification.Create(
            _tenantId,
            _userId,
            "Some title",
            null!,
            InAppNotificationType.AppointmentCreated);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MarkAsRead_ShouldSetIsReadTrue()
    {
        var notification = InAppNotification.Create(
            _tenantId,
            _userId,
            "New appointment",
            "A new appointment has been scheduled.",
            InAppNotificationType.AppointmentCreated);

        notification.MarkAsRead();

        notification.IsRead.Should().BeTrue();
        notification.UpdatedAt.Should().NotBeNull();
    }
}
