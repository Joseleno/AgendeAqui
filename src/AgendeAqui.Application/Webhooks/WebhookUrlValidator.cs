using System.Net;
using System.Net.Sockets;

namespace AgendeAqui.Application.Webhooks;

public static class WebhookUrlValidator
{
    private static readonly IPNetwork[] PrivateNetworks =
    [
        // IPv4
        new(IPAddress.Parse("127.0.0.0"), 8),
        new(IPAddress.Parse("10.0.0.0"), 8),
        new(IPAddress.Parse("172.16.0.0"), 12),
        new(IPAddress.Parse("192.168.0.0"), 16),
        new(IPAddress.Parse("169.254.0.0"), 16),
        new(IPAddress.Parse("0.0.0.0"), 8),

        // IPv6
        new(IPAddress.Parse("::1"), 128),
        new(IPAddress.Parse("fe80::"), 10),   // link-local
        new(IPAddress.Parse("fc00::"), 7),    // unique local

        // IPv6-mapped IPv4
        new(IPAddress.Parse("::ffff:127.0.0.0"), 104),
        new(IPAddress.Parse("::ffff:10.0.0.0"), 104),
        new(IPAddress.Parse("::ffff:172.16.0.0"), 108),
        new(IPAddress.Parse("::ffff:192.168.0.0"), 112),
        new(IPAddress.Parse("::ffff:169.254.0.0"), 112),
        new(IPAddress.Parse("::ffff:0.0.0.0"), 104),
    ];

    /// <summary>
    /// Returns true if the URL is a valid absolute HTTP or HTTPS URI.
    /// </summary>
    public static bool IsValidHttpUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// Returns true if the URL scheme is not HTTP/HTTPS.
    /// </summary>
    public static bool HasInvalidScheme(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        return uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps;
    }

    /// <summary>
    /// Returns true if the given IP address falls within a private or reserved range.
    /// Also checks IPv6-mapped IPv4 addresses by mapping to IPv4 before comparison.
    /// </summary>
    public static bool IsPrivateOrReservedIp(IPAddress address)
    {
        if (IPAddress.IsLoopback(address))
            return true;

        // Check the mapped IPv4 address as well for IPv6-mapped IPv4
        if (address.IsIPv4MappedToIPv6)
        {
            var ipv4 = address.MapToIPv4();
            if (IPAddress.IsLoopback(ipv4))
                return true;

            foreach (var network in PrivateNetworks)
            {
                if (network.Contains(ipv4))
                    return true;
            }
        }

        foreach (var network in PrivateNetworks)
        {
            if (network.Contains(address))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Resolves the URL's host via DNS and returns true if any resolved address
    /// is private or reserved. Uses a 5-second timeout. Fails closed (returns true on error).
    /// </summary>
    public static async Task<bool> ResolvesToPrivateIpAsync(string? url, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return true;

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            var addresses = await Dns.GetHostAddressesAsync(uri.DnsSafeHost, cts.Token);

            foreach (var address in addresses)
            {
                if (IsPrivateOrReservedIp(address))
                    return true;
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // DNS timeout — fail closed
            return true;
        }
        catch (SocketException)
        {
            // DNS resolution failed — fail closed
            return true;
        }

        return false;
    }
}
