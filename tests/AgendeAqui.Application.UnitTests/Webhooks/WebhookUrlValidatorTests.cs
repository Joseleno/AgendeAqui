using System.Net;
using AgendeAqui.Application.Webhooks;
using FluentAssertions;

namespace AgendeAqui.Application.UnitTests.Webhooks;

public class WebhookUrlValidatorTests
{
    // ── IsValidHttpUrl ──────────────────────────────────────────────────

    [Fact]
    public void IsValidHttpUrl_WithValidHttps_ShouldReturnTrue()
    {
        WebhookUrlValidator.IsValidHttpUrl("https://example.com/webhook").Should().BeTrue();
    }

    [Fact]
    public void IsValidHttpUrl_WithValidHttp_ShouldReturnTrue()
    {
        WebhookUrlValidator.IsValidHttpUrl("http://example.com/webhook").Should().BeTrue();
    }

    [Fact]
    public void IsValidHttpUrl_WithNull_ShouldReturnFalse()
    {
        WebhookUrlValidator.IsValidHttpUrl(null).Should().BeFalse();
    }

    [Fact]
    public void IsValidHttpUrl_WithEmpty_ShouldReturnFalse()
    {
        WebhookUrlValidator.IsValidHttpUrl("").Should().BeFalse();
    }

    [Fact]
    public void IsValidHttpUrl_WithNotAUrl_ShouldReturnFalse()
    {
        WebhookUrlValidator.IsValidHttpUrl("not-a-url").Should().BeFalse();
    }

    [Fact]
    public void IsValidHttpUrl_WithFtp_ShouldReturnFalse()
    {
        WebhookUrlValidator.IsValidHttpUrl("ftp://example.com").Should().BeFalse();
    }

    [Fact]
    public void IsValidHttpUrl_WithFileScheme_ShouldReturnFalse()
    {
        WebhookUrlValidator.IsValidHttpUrl("file:///etc/passwd").Should().BeFalse();
    }

    // ── HasInvalidScheme ────────────────────────────────────────────────

    [Fact]
    public void HasInvalidScheme_WithNull_ShouldReturnFalse()
    {
        WebhookUrlValidator.HasInvalidScheme(null).Should().BeFalse();
    }

    [Fact]
    public void HasInvalidScheme_WithEmpty_ShouldReturnFalse()
    {
        WebhookUrlValidator.HasInvalidScheme("").Should().BeFalse();
    }

    [Fact]
    public void HasInvalidScheme_WithHttp_ShouldReturnFalse()
    {
        WebhookUrlValidator.HasInvalidScheme("http://example.com").Should().BeFalse();
    }

    [Fact]
    public void HasInvalidScheme_WithHttps_ShouldReturnFalse()
    {
        WebhookUrlValidator.HasInvalidScheme("https://example.com").Should().BeFalse();
    }

    [Fact]
    public void HasInvalidScheme_WithFtp_ShouldReturnTrue()
    {
        WebhookUrlValidator.HasInvalidScheme("ftp://example.com").Should().BeTrue();
    }

    [Fact]
    public void HasInvalidScheme_WithFile_ShouldReturnTrue()
    {
        WebhookUrlValidator.HasInvalidScheme("file:///etc/passwd").Should().BeTrue();
    }

    [Fact]
    public void HasInvalidScheme_WithGopher_ShouldReturnTrue()
    {
        WebhookUrlValidator.HasInvalidScheme("gopher://example.com").Should().BeTrue();
    }

    // ── IsPrivateOrReservedIp ───────────────────────────────────────────

    [Fact]
    public void IsPrivateOrReservedIp_Loopback_ShouldReturnTrue()
    {
        WebhookUrlValidator.IsPrivateOrReservedIp(IPAddress.Parse("127.0.0.1")).Should().BeTrue();
    }

    [Fact]
    public void IsPrivateOrReservedIp_10Network_ShouldReturnTrue()
    {
        WebhookUrlValidator.IsPrivateOrReservedIp(IPAddress.Parse("10.0.0.1")).Should().BeTrue();
    }

    [Fact]
    public void IsPrivateOrReservedIp_172_16_ShouldReturnTrue()
    {
        WebhookUrlValidator.IsPrivateOrReservedIp(IPAddress.Parse("172.16.0.1")).Should().BeTrue();
    }

    [Fact]
    public void IsPrivateOrReservedIp_192_168_ShouldReturnTrue()
    {
        WebhookUrlValidator.IsPrivateOrReservedIp(IPAddress.Parse("192.168.1.1")).Should().BeTrue();
    }

    [Fact]
    public void IsPrivateOrReservedIp_LinkLocal_ShouldReturnTrue()
    {
        WebhookUrlValidator.IsPrivateOrReservedIp(IPAddress.Parse("169.254.1.1")).Should().BeTrue();
    }

    [Fact]
    public void IsPrivateOrReservedIp_Ipv6Loopback_ShouldReturnTrue()
    {
        WebhookUrlValidator.IsPrivateOrReservedIp(IPAddress.Parse("::1")).Should().BeTrue();
    }

    [Fact]
    public void IsPrivateOrReservedIp_Ipv6LinkLocal_ShouldReturnTrue()
    {
        WebhookUrlValidator.IsPrivateOrReservedIp(IPAddress.Parse("fe80::1")).Should().BeTrue();
    }

    [Fact]
    public void IsPrivateOrReservedIp_Ipv6UniqueLocal_ShouldReturnTrue()
    {
        WebhookUrlValidator.IsPrivateOrReservedIp(IPAddress.Parse("fc00::1")).Should().BeTrue();
    }

    [Fact]
    public void IsPrivateOrReservedIp_Ipv6MappedLoopback_ShouldReturnTrue()
    {
        WebhookUrlValidator.IsPrivateOrReservedIp(IPAddress.Parse("::ffff:127.0.0.1")).Should().BeTrue();
    }

    [Fact]
    public void IsPrivateOrReservedIp_PublicIp_ShouldReturnFalse()
    {
        WebhookUrlValidator.IsPrivateOrReservedIp(IPAddress.Parse("8.8.8.8")).Should().BeFalse();
    }

    // ── ResolvesToPrivateIpAsync ─────────────────────────────────────────

    [Fact]
    public async Task ResolvesToPrivateIpAsync_Localhost_ShouldReturnTrue()
    {
        var result = await WebhookUrlValidator.ResolvesToPrivateIpAsync("http://localhost", CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ResolvesToPrivateIpAsync_Null_ShouldReturnFalse()
    {
        var result = await WebhookUrlValidator.ResolvesToPrivateIpAsync(null, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ResolvesToPrivateIpAsync_Empty_ShouldReturnFalse()
    {
        var result = await WebhookUrlValidator.ResolvesToPrivateIpAsync("", CancellationToken.None);

        result.Should().BeFalse();
    }
}
