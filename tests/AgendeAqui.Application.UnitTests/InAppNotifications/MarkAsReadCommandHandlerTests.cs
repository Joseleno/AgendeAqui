using AgendeAqui.Application.InAppNotifications.MarkAsRead;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.InAppNotifications;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.InAppNotifications;

public sealed class MarkAsReadCommandHandlerTests
{
    private readonly IInAppNotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly MarkAsReadCommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _tenantId = Guid.NewGuid();

    public MarkAsReadCommandHandlerTests()
    {
        _notificationRepository = Substitute.For<IInAppNotificationRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _currentUser = Substitute.For<ICurrentUser>();
        _currentUser.UserId.Returns(_userId);

        _handler = new MarkAsReadCommandHandler(
            _notificationRepository,
            _unitOfWork,
            _currentUser);
    }

    [Fact]
    public async Task Handle_ExistingNotification_ShouldMarkAsRead()
    {
        var notification = InAppNotification.Create(
            _tenantId,
            _userId,
            "Test title",
            "Test message",
            InAppNotificationType.AppointmentCreated);

        _notificationRepository.GetByIdAsync(notification.Id, Arg.Any<CancellationToken>())
            .Returns(notification);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = new MarkAsReadCommand(notification.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        notification.IsRead.Should().BeTrue();
        _notificationRepository.Received(1).Update(notification);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NotFound_ShouldReturnError()
    {
        _notificationRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((InAppNotification?)null);

        var command = new MarkAsReadCommand(Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(InAppNotificationErrors.NotFound);
    }
}
