namespace AgendeAqui.Api.Auth;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";
    public string Issuer { get; init; } = default!;
    public string Audience { get; init; } = default!;
    public string SecretKey { get; init; } = default!;
    public int ExpirationMinutes { get; init; } = 60;
}
