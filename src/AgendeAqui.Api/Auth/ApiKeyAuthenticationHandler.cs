using AgendeAqui.Application.Abstractions.Data;
using Dapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;

namespace AgendeAqui.Api.Auth;

public sealed class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    ISqlConnectionFactory sqlConnectionFactory)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private const string ApiKeyHeaderName = "X-Api-Key";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeaderValues))
            return AuthenticateResult.NoResult();

        var rawKey = apiKeyHeaderValues.ToString();
        if (string.IsNullOrWhiteSpace(rawKey))
            return AuthenticateResult.NoResult();

        var keyHash = ComputeSha256Hash(rawKey);

        using var connection = await sqlConnectionFactory.CreateConnectionAsync(Context.RequestAborted);

        var command = new CommandDefinition(
            "SELECT id, tenant_id, name " +
            "FROM api_keys WHERE key_hash = @KeyHash AND is_active = true " +
            "AND (expires_at IS NULL OR expires_at > NOW() AT TIME ZONE 'UTC')",
            new { KeyHash = keyHash },
            cancellationToken: Context.RequestAborted);

        var apiKey = await connection.QuerySingleOrDefaultAsync<ApiKeyRecord>(command);

        if (apiKey is null)
            return AuthenticateResult.Fail("Invalid API key.");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, apiKey.Id.ToString()),
            new Claim("sub", apiKey.Id.ToString()),
            new Claim("tenant_id", apiKey.TenantId.ToString()),
            new Claim(ClaimTypes.Role, "ApiKey"),
            new Claim("role", "ApiKey"),
            new Claim("api_key_name", apiKey.Name)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    private static string ComputeSha256Hash(string rawKey)
    {
        var bytes = Encoding.UTF8.GetBytes(rawKey);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexStringLower(hashBytes);
    }

    private sealed record ApiKeyRecord(
        Guid Id,
        Guid TenantId,
        string Name);
}
