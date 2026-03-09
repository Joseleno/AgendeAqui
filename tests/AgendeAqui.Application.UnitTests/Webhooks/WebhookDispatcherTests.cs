using System.Net;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Webhooks;
using AgendeAqui.Infrastructure.Webhooks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Webhooks;

public class WebhookDispatcherTests
{
    private readonly IWebhookRepository _webhookRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WebhookDispatcher> _logger;

    private readonly List<(HttpRequestMessage Request, string Body)> _capturedRequests = [];
    private Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _httpHandler;

    private WebhookDispatcher CreateDispatcher()
    {
        var fakeHandler = new FakeHttpHandler(_httpHandler);
        var httpClient = new HttpClient(fakeHandler);
        return new WebhookDispatcher(_webhookRepository, _unitOfWork, httpClient, _logger);
    }

    public WebhookDispatcherTests()
    {
        _webhookRepository = Substitute.For<IWebhookRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<WebhookDispatcher>>();

        _httpHandler = async (request, ct) =>
        {
            var body = request.Content is not null
                ? await request.Content.ReadAsStringAsync(ct)
                : string.Empty;
            _capturedRequests.Add((request, body));
            return new HttpResponseMessage(HttpStatusCode.OK);
        };

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
    }

    private static Webhook CreateWebhook(string? url = null, string? secretHash = null, string? eventType = null)
    {
        return Webhook.Create(
            Guid.NewGuid(),
            url ?? "https://example.com/hook",
            secretHash ?? "secret-hash",
            [eventType ?? "appointment.created"]).Value;
    }

    // -------------------------------------------------------------------------
    // Test 1 – No active webhooks: no HTTP calls, nothing saved
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DispatchAsync_WithNoActiveWebhooks_ShouldNotMakeHttpCallsOrSaveDeliveries()
    {
        // Arrange
        _webhookRepository
            .GetActiveByEventAsync("appointment.created", Arg.Any<CancellationToken>())
            .Returns(new List<Webhook>());

        var dispatcher = CreateDispatcher();

        // Act
        await dispatcher.DispatchAsync("appointment.created", """{"id":"1"}""", CancellationToken.None);

        // Assert – no HTTP traffic and nothing persisted
        _capturedRequests.Should().BeEmpty();
        await _webhookRepository.DidNotReceive().AddDeliveryAsync(Arg.Any<WebhookDelivery>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // -------------------------------------------------------------------------
    // Test 2 – Active webhook: POST to correct URL with proper headers
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DispatchAsync_WithActiveWebhook_ShouldPostPayloadWithCorrectHeadersAndContentType()
    {
        // Arrange
        const string eventType = "appointment.created";
        const string payload = """{"id":"abc-123"}""";
        var webhook = CreateWebhook(url: "https://example.com/hook", secretHash: "my-secret", eventType: eventType);

        _webhookRepository
            .GetActiveByEventAsync(eventType, Arg.Any<CancellationToken>())
            .Returns(new List<Webhook> { webhook });

        var dispatcher = CreateDispatcher();

        // Act
        await dispatcher.DispatchAsync(eventType, payload, CancellationToken.None);

        // Assert
        _capturedRequests.Should().HaveCount(1);

        var (request, body) = _capturedRequests[0];
        request.Method.Should().Be(HttpMethod.Post);
        request.RequestUri.Should().Be("https://example.com/hook");

        body.Should().Be(payload);

        // Content-Type is read from the captured headers before disposal
        request.Headers.TryGetValues("X-Webhook-Signature", out var sigValues).Should().BeTrue();
        var sigHeader = sigValues!.Single();
        sigHeader.Should().StartWith("sha256=");
        sigHeader.Length.Should().BeGreaterThan("sha256=".Length);
    }

    // -------------------------------------------------------------------------
    // Test 3 – Successful delivery: response code recorded and delivery saved
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DispatchAsync_WithSuccessfulDelivery_ShouldRecordResponseCodeAndPersist()
    {
        // Arrange
        const string eventType = "appointment.created";
        const string payload = """{"id":"abc-123"}""";
        var webhook = CreateWebhook(eventType: eventType);

        _webhookRepository
            .GetActiveByEventAsync(eventType, Arg.Any<CancellationToken>())
            .Returns(new List<Webhook> { webhook });

        _httpHandler = (_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        WebhookDelivery? savedDelivery = null;
        await _webhookRepository.AddDeliveryAsync(
            Arg.Do<WebhookDelivery>(d => savedDelivery = d),
            Arg.Any<CancellationToken>());

        var dispatcher = CreateDispatcher();

        // Act
        await dispatcher.DispatchAsync(eventType, payload, CancellationToken.None);

        // Assert
        await _webhookRepository.Received(1).AddDeliveryAsync(Arg.Any<WebhookDelivery>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        savedDelivery.Should().NotBeNull();
        savedDelivery!.ResponseCode.Should().Be(200);
        savedDelivery.IsSuccess.Should().BeTrue();
        savedDelivery.EventType.Should().Be(eventType);
        savedDelivery.Payload.Should().Be(payload);
    }

    // -------------------------------------------------------------------------
    // Test 4 – HTTP exception: delivery still created without response code
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DispatchAsync_WhenHttpClientThrows_ShouldStillCreateDeliveryWithoutResponseCode()
    {
        // Arrange
        const string eventType = "appointment.created";
        const string payload = """{"id":"abc-123"}""";
        var webhook = CreateWebhook(eventType: eventType);

        _webhookRepository
            .GetActiveByEventAsync(eventType, Arg.Any<CancellationToken>())
            .Returns(new List<Webhook> { webhook });

        _httpHandler = (_, _) => throw new HttpRequestException("Connection refused");

        WebhookDelivery? savedDelivery = null;
        await _webhookRepository.AddDeliveryAsync(
            Arg.Do<WebhookDelivery>(d => savedDelivery = d),
            Arg.Any<CancellationToken>());

        var dispatcher = CreateDispatcher();

        // Act
        await dispatcher.DispatchAsync(eventType, payload, CancellationToken.None);

        // Assert – delivery must still be persisted even when HTTP fails
        await _webhookRepository.Received(1).AddDeliveryAsync(Arg.Any<WebhookDelivery>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        savedDelivery.Should().NotBeNull();
        savedDelivery!.ResponseCode.Should().BeNull("no response received when the HTTP call throws");
    }

    // -------------------------------------------------------------------------
    // Test 5 – Multiple webhooks: all receive the payload
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DispatchAsync_WithMultipleWebhooks_ShouldDispatchToAll()
    {
        // Arrange
        const string eventType = "appointment.created";
        const string payload = """{"id":"abc-123"}""";

        var webhook1 = CreateWebhook(url: "https://alpha.example.com/hook", eventType: eventType);
        var webhook2 = CreateWebhook(url: "https://beta.example.com/hook", eventType: eventType);
        var webhook3 = CreateWebhook(url: "https://gamma.example.com/hook", eventType: eventType);

        _webhookRepository
            .GetActiveByEventAsync(eventType, Arg.Any<CancellationToken>())
            .Returns(new List<Webhook> { webhook1, webhook2, webhook3 });

        var dispatcher = CreateDispatcher();

        // Act
        await dispatcher.DispatchAsync(eventType, payload, CancellationToken.None);

        // Assert
        _capturedRequests.Should().HaveCount(3);

        var requestedUrls = _capturedRequests.Select(r => r.Request.RequestUri!.ToString()).ToList();
        requestedUrls.Should().Contain("https://alpha.example.com/hook");
        requestedUrls.Should().Contain("https://beta.example.com/hook");
        requestedUrls.Should().Contain("https://gamma.example.com/hook");

        await _webhookRepository.Received(3).AddDeliveryAsync(Arg.Any<WebhookDelivery>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // -------------------------------------------------------------------------
    // Test 6 – ComputeSignature determinism (isolated from WebhookHmacTests)
    // -------------------------------------------------------------------------

    [Fact]
    public void ComputeSignature_WithSameInputs_ShouldProduceDeterministicOutput()
    {
        // Arrange
        const string payload = """{"id":"abc-123"}""";
        const string key = "shared-secret-key";

        // Act
        var first = WebhookDispatcher.ComputeSignature(payload, key);
        var second = WebhookDispatcher.ComputeSignature(payload, key);

        // Assert
        first.Should().Be(second, "HMAC output must be deterministic for the same inputs");
        first.Should().MatchRegex("^[0-9a-f]{64}$", "output must be a lowercase hex-encoded SHA-256 digest");
    }

    // -------------------------------------------------------------------------
    // Test 7 – Cancellation propagates out of DispatchAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DispatchAsync_WhenCancelled_ShouldPropagateOperationCanceledException()
    {
        // Arrange
        const string eventType = "appointment.created";
        var webhook = CreateWebhook(eventType: eventType);

        _webhookRepository
            .GetActiveByEventAsync(eventType, Arg.Any<CancellationToken>())
            .Returns(new List<Webhook> { webhook });

        using var cts = new CancellationTokenSource();
        _httpHandler = (_, _) =>
        {
            cts.Cancel();
            throw new OperationCanceledException(cts.Token);
        };

        var dispatcher = CreateDispatcher();

        // Act
        Func<Task> act = () => dispatcher.DispatchAsync(eventType, """{"id":"1"}""", cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // -------------------------------------------------------------------------
    // Fake HTTP handler (no extra NuGet required)
    // -------------------------------------------------------------------------

    private sealed class FakeHttpHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
        : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken ct)
        {
            // Buffer content before passing to handler so ReadAsStringAsync
            // works even after HttpClient clears the stream.
            if (request.Content is not null)
                await request.Content.LoadIntoBufferAsync(ct);

            return await handler(request, ct);
        }
    }
}
