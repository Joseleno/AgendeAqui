using System.Security.Cryptography;
using System.Text;
using AgendeAqui.Application.Webhooks.RegisterWebhook;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Webhooks;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Webhooks;

public class RegisterWebhookCommandHandlerTests
{
    private readonly IWebhookRepository _webhookRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly RegisterWebhookCommandHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public RegisterWebhookCommandHandlerTests()
    {
        _webhookRepository = Substitute.For<IWebhookRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _tenantProvider.GetTenantId().Returns(_tenantId);

        _handler = new RegisterWebhookCommandHandler(_webhookRepository, _unitOfWork, _tenantProvider);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateWebhookAndReturnId()
    {
        // Arrange
        var command = new RegisterWebhookCommand(
            "https://example.com/webhook",
            "my-secret",
            ["appointment.created"]);

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        Webhook? capturedWebhook = null;
        await _webhookRepository.AddAsync(
            Arg.Do<Webhook>(w => capturedWebhook = w),
            Arg.Any<CancellationToken>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        capturedWebhook.Should().NotBeNull();
        result.Value.Should().Be(capturedWebhook!.Id);

        _tenantProvider.Received(1).GetTenantId();
        await _webhookRepository.Received(1).AddAsync(Arg.Any<Webhook>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldHashSecretWithSha256()
    {
        // Arrange
        const string secret = "my-secret";
        var command = new RegisterWebhookCommand(
            "https://example.com/webhook",
            secret,
            ["appointment.created"]);

        var expectedHash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(secret)));

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        Webhook? capturedWebhook = null;
        await _webhookRepository.AddAsync(
            Arg.Do<Webhook>(w => capturedWebhook = w),
            Arg.Any<CancellationToken>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedWebhook.Should().NotBeNull();
        capturedWebhook!.SecretHash.Should().Be(expectedHash);
    }

    [Fact]
    public async Task Handle_WhenWebhookCreateFails_ShouldPropagateError()
    {
        // Arrange — empty events list causes Webhook.Create to fail with InvalidEvents
        var command = new RegisterWebhookCommand(
            "https://example.com/webhook",
            "my-secret",
            []);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WebhookErrors.InvalidEvents);

        await _webhookRepository.DidNotReceive().AddAsync(Arg.Any<Webhook>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidUrl_ShouldPropagateError()
    {
        // Arrange — invalid URL causes Webhook.Create to fail with InvalidUrl
        var command = new RegisterWebhookCommand(
            "not-a-url",
            "my-secret",
            ["appointment.created"]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WebhookErrors.InvalidUrl);

        await _webhookRepository.DidNotReceive().AddAsync(Arg.Any<Webhook>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
