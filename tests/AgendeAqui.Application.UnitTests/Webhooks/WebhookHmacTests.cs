using System.Security.Cryptography;
using System.Text;
using AgendeAqui.Infrastructure.Webhooks;
using FluentAssertions;

namespace AgendeAqui.Application.UnitTests.Webhooks;

public class WebhookHmacTests
{
    [Fact]
    public void ComputeSignature_WithKnownInput_ShouldProduceExpectedSignature()
    {
        // Arrange
        const string key = "my-super-secret-webhook-key-32ch";
        const string payload = """{"event":"appointment.created","id":"abc-123"}""";

        // Act
        var signature = WebhookDispatcher.ComputeSignature(payload, key);

        // Assert — cross-validate with HMACSHA256 class form
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var expectedBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var expected = Convert.ToHexString(expectedBytes).ToLowerInvariant();

        signature.Should().Be(expected);
        signature.Should().HaveLength(64);
        signature.Should().MatchRegex("^[0-9a-f]{64}$");
    }

    [Fact]
    public void ComputeSignature_WithDifferentKeys_ShouldProduceDifferentSignatures()
    {
        const string payload = """{"event":"appointment.created"}""";

        var sig1 = WebhookDispatcher.ComputeSignature(payload, "key-one-32-characters-long-paddd");
        var sig2 = WebhookDispatcher.ComputeSignature(payload, "key-two-32-characters-long-paddd");

        sig1.Should().NotBe(sig2);
    }

    [Fact]
    public void ComputeSignature_WithDifferentPayloads_ShouldProduceDifferentSignatures()
    {
        const string key = "my-super-secret-webhook-key-32ch";

        var sig1 = WebhookDispatcher.ComputeSignature("""{"event":"appointment.created"}""", key);
        var sig2 = WebhookDispatcher.ComputeSignature("""{"event":"appointment.cancelled"}""", key);

        sig1.Should().NotBe(sig2);
    }

    [Fact]
    public void ComputeSignature_WithSameInputTwice_ShouldProduceSameSignature()
    {
        const string key = "my-super-secret-webhook-key-32ch";
        const string payload = """{"event":"appointment.created"}""";

        var sig1 = WebhookDispatcher.ComputeSignature(payload, key);
        var sig2 = WebhookDispatcher.ComputeSignature(payload, key);

        sig1.Should().Be(sig2);
    }
}
