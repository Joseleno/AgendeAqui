using AgendeAqui.Domain.Webhooks;
using FluentAssertions;

namespace AgendeAqui.Domain.UnitTests.Webhooks;

public class WebhookTests
{
    private static List<string> ValidEvents() => ["appointment.created", "appointment.cancelled"];

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var tenantId = Guid.NewGuid();
        var url = "https://example.com/webhook";
        var secretHash = "my-secret-hash";
        var events = ValidEvents();

        var result = Webhook.Create(tenantId, url, secretHash, events);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBe(Guid.Empty);
        result.Value.TenantId.Should().Be(tenantId);
        result.Value.Url.Should().Be(url);
        result.Value.SecretHash.Should().Be(secretHash);
        result.Value.Events.Should().BeEquivalentTo(events);
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyTenantId_ShouldFail()
    {
        var result = Webhook.Create(Guid.Empty, "https://example.com/webhook", "secret", ValidEvents());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WebhookErrors.InvalidTenant);
    }

    [Fact]
    public void Create_WithEmptyUrl_ShouldFail()
    {
        var result = Webhook.Create(Guid.NewGuid(), "", "secret", ValidEvents());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WebhookErrors.InvalidUrl);
    }

    [Fact]
    public void Create_WithWhitespaceUrl_ShouldFail()
    {
        var result = Webhook.Create(Guid.NewGuid(), "   ", "secret", ValidEvents());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WebhookErrors.InvalidUrl);
    }

    [Fact]
    public void Create_WithInvalidUrl_ShouldFail()
    {
        var result = Webhook.Create(Guid.NewGuid(), "not-a-valid-url", "secret", ValidEvents());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WebhookErrors.InvalidUrl);
    }

    [Fact]
    public void Create_WithNonHttpScheme_ShouldFail()
    {
        var result = Webhook.Create(Guid.NewGuid(), "ftp://example.com/hook", "secret", ValidEvents());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WebhookErrors.InvalidUrl);
    }

    [Fact]
    public void Create_WithHttpUrl_ShouldSucceed()
    {
        var result = Webhook.Create(Guid.NewGuid(), "http://example.com/webhook", "secret", ValidEvents());

        result.IsSuccess.Should().BeTrue();
        result.Value.Url.Should().Be("http://example.com/webhook");
    }

    [Fact]
    public void Create_WithUrlExceedingMaxLength_ShouldFail()
    {
        var longUrl = "https://example.com/" + new string('a', 2030);

        var result = Webhook.Create(Guid.NewGuid(), longUrl, "secret", ValidEvents());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WebhookErrors.InvalidUrl);
    }

    [Fact]
    public void Create_WithEmptySecret_ShouldFail()
    {
        var result = Webhook.Create(Guid.NewGuid(), "https://example.com/webhook", "", ValidEvents());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WebhookErrors.InvalidSecret);
    }

    [Fact]
    public void Create_WithWhitespaceSecret_ShouldFail()
    {
        var result = Webhook.Create(Guid.NewGuid(), "https://example.com/webhook", "   ", ValidEvents());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WebhookErrors.InvalidSecret);
    }

    [Fact]
    public void Create_WithEmptyEvents_ShouldFail()
    {
        var result = Webhook.Create(Guid.NewGuid(), "https://example.com/webhook", "secret", []);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WebhookErrors.InvalidEvents);
    }

    [Fact]
    public void Create_WithNullEvents_ShouldFail()
    {
        var result = Webhook.Create(Guid.NewGuid(), "https://example.com/webhook", "secret", null!);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(WebhookErrors.InvalidEvents);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveTrue()
    {
        var webhook = Webhook.Create(Guid.NewGuid(), "https://example.com/webhook", "secret", ValidEvents()).Value;
        webhook.Deactivate();
        webhook.IsActive.Should().BeFalse();

        webhook.Activate();

        webhook.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var webhook = Webhook.Create(Guid.NewGuid(), "https://example.com/webhook", "secret", ValidEvents()).Value;
        webhook.IsActive.Should().BeTrue();

        webhook.Deactivate();

        webhook.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldUpdateUpdatedAt()
    {
        var webhook = Webhook.Create(Guid.NewGuid(), "https://example.com/webhook", "secret", ValidEvents()).Value;
        webhook.Deactivate();
        var before = DateTime.UtcNow.AddSeconds(-1);

        webhook.Activate();

        webhook.UpdatedAt.Should().NotBeNull();
        webhook.UpdatedAt!.Value.Should().BeAfter(before);
    }

    [Fact]
    public void Deactivate_ShouldUpdateUpdatedAt()
    {
        var webhook = Webhook.Create(Guid.NewGuid(), "https://example.com/webhook", "secret", ValidEvents()).Value;
        var before = DateTime.UtcNow.AddSeconds(-1);

        webhook.Deactivate();

        webhook.UpdatedAt.Should().NotBeNull();
        webhook.UpdatedAt!.Value.Should().BeAfter(before);
    }
}
