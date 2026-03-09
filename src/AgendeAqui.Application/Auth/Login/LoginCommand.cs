using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Auth.Login;

public sealed record LoginCommand(string Email, string Password) : ICommand<LoginResponse>;
