using AgendeAqui.Application.Webhooks.UpdateWebhook;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Webhooks;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Webhooks;

public class UpdateWebhookCommandHandlerTests
{
    private readonly IWebhookRepository _webhookRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdateWebhookCommandHandler _handler;

    public UpdateWebhookCommandHandlerTests()
    {
        _webhookRepository = Substitute.For<IWebhookRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new UpdateWebhookCommandHandler(_webhookRepository, _unitOfWork);
    }

    private static Webhook CreateWebhook()
    {
        return Webhook.Create(
            Guid.NewGuid(),
            "https://original.com/hook",
            "secret-hash-value",
            ["appointment.created"]).Value;
    }

    [Fact]
    public async Task Handle_WithExistingWebhook_ShouldUpdateAndSave()
    {
        // Arrange
        var webhook = CreateWebhook();
        var command = new UpdateWebhookCommand(
            webhook.Id, "https://new-url.com/hook", ["appointment.cancelled", "appointment.created"], true);

        _webhookRepository.GetByIdAsync(webhook.Id, Arg.Any<CancellationToken>()).Returns(webhook);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        webhook.Url.Should().Be("https://new-url.com/hook");
        webhook.Events.Should().HaveCount(2);
        _webhookRepository.Received(1).Update(webhook);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentWebhook_ShouldReturnNotFound()
    {
        // Arrange
        var command = new UpdateWebhookCommand(Guid.NewGuid(), "https://test.com", ["event"], true);
        _webhookRepository.GetByIdAsync(command.WebhookId, Arg.Any<CancellationToken>()).Returns((Webhook?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WebhookErrors.NotFound);
        _webhookRepository.DidNotReceive().Update(Arg.Any<Webhook>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidUrl_ShouldReturnError()
    {
        // Arrange
        var webhook = CreateWebhook();
        var command = new UpdateWebhookCommand(webhook.Id, "not-a-valid-url", ["event"], true);

        _webhookRepository.GetByIdAsync(webhook.Id, Arg.Any<CancellationToken>()).Returns(webhook);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WebhookErrors.InvalidUrl);
        _webhookRepository.DidNotReceive().Update(Arg.Any<Webhook>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptyEvents_ShouldReturnError()
    {
        // Arrange
        var webhook = CreateWebhook();
        var command = new UpdateWebhookCommand(webhook.Id, "https://test.com", [], true);

        _webhookRepository.GetByIdAsync(webhook.Id, Arg.Any<CancellationToken>()).Returns(webhook);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WebhookErrors.InvalidEvents);
        _webhookRepository.DidNotReceive().Update(Arg.Any<Webhook>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DeactivatingWebhook_ShouldPersistIsActiveFalse()
    {
        // Arrange
        var webhook = CreateWebhook();
        var command = new UpdateWebhookCommand(webhook.Id, "https://test.com/hook", ["appointment.created"], false);

        _webhookRepository.GetByIdAsync(webhook.Id, Arg.Any<CancellationToken>()).Returns(webhook);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        webhook.IsActive.Should().BeFalse();
        _webhookRepository.Received(1).Update(webhook);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
