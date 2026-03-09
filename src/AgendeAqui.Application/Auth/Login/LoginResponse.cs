namespace AgendeAqui.Application.Auth.Login;

public sealed record LoginResponse(string AccessToken, string RefreshToken, int ExpiresInMinutes);
