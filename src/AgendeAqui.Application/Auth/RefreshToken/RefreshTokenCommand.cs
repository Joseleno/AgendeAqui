using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Auth.Login;

namespace AgendeAqui.Application.Auth.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<LoginResponse>;
