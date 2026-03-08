using AgendeAqui.Application.Webhooks.DeleteWebhook;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Webhooks;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Webhooks;

public class DeleteWebhookCommandHandlerTests
{
    private readonly IWebhookRepository _webhookRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeleteWebhookCommandHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public DeleteWebhookCommandHandlerTests()
    {
        _webhookRepository = Substitute.For<IWebhookRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new DeleteWebhookCommandHandler(_webhookRepository, _unitOfWork);
    }

    private Webhook CreateWebhook() =>
        Webhook.Create(_tenantId, "https://example.com/webhook", "hashed-secret", ["appointment.created"]).Value;

    [Fact]
    public async Task Handle_ExistingWebhook_ShouldDeleteAndReturnSuccess()
    {
        // Arrange
        var webhook = CreateWebhook();
        var command = new DeleteWebhookCommand(webhook.Id);

        _webhookRepository.GetByIdAsync(webhook.Id, Arg.Any<CancellationToken>()).Returns(webhook);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _webhookRepository.Received(1).Remove(webhook);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NonExistentWebhook_ShouldReturnNotFound()
    {
        // Arrange
        var webhookId = Guid.NewGuid();
        var command = new DeleteWebhookCommand(webhookId);

        _webhookRepository.GetByIdAsync(webhookId, Arg.Any<CancellationToken>()).Returns((Webhook?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WebhookErrors.NotFound);

        _webhookRepository.DidNotReceive().Remove(Arg.Any<Webhook>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
