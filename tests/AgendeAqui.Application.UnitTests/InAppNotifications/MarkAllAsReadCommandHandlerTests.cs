using AgendeAqui.Application.InAppNotifications.MarkAllAsRead;
using AgendeAqui.Domain.Abstractions;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.InAppNotifications;

public sealed class MarkAllAsReadCommandHandlerTests
{
    private readonly IInAppNotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly MarkAllAsReadCommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public MarkAllAsReadCommandHandlerTests()
    {
        _notificationRepository = Substitute.For<IInAppNotificationRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _currentUser = Substitute.For<ICurrentUser>();
        _currentUser.UserId.Returns(_userId);

        _handler = new MarkAllAsReadCommandHandler(
            _notificationRepository,
            _unitOfWork,
            _currentUser);
    }

    [Fact]
    public async Task Handle_ShouldCallMarkAllAsRead()
    {
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = new MarkAllAsReadCommand();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _notificationRepository.Received(1).MarkAllAsReadAsync(_userId, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
