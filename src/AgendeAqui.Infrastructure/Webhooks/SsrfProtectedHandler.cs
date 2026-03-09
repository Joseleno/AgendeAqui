using System.Net;
using System.Net.Sockets;
using AgendeAqui.Application.Webhooks;

namespace AgendeAqui.Infrastructure.Webhooks;

/// <summary>
/// Provides a <see cref="SocketsHttpHandler.ConnectCallback"/> that resolves DNS
/// and blocks connections to private/reserved IP ranges (SSRF protection).
/// </summary>
internal static class SsrfProtectedHandler
{
    public static async ValueTask<Stream> ConnectCallback(
        SocketsHttpConnectionContext context,
        CancellationToken cancellationToken)
    {
        var addresses = await Dns.GetHostAddressesAsync(
            context.DnsEndPoint.Host, cancellationToken);

        foreach (var address in addresses)
        {
            if (WebhookUrlValidator.IsPrivateOrReservedIp(address))
            {
                throw new HttpRequestException(
                    $"Connection to private/reserved IP address {address} is blocked (SSRF protection).");
            }
        }

        var socket = new Socket(SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = true,
        };

        try
        {
            await socket.ConnectAsync(addresses, context.DnsEndPoint.Port, cancellationToken);
            return new NetworkStream(socket, ownsSocket: true);
        }
        catch
        {
            socket.Dispose();
            throw;
        }
    }
}
