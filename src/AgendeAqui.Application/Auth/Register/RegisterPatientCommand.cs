using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Auth.Login;

namespace AgendeAqui.Application.Auth.Register;

public sealed record RegisterPatientCommand(
    string Name,
    string Email,
    string Phone,
    string Password) : ICommand<LoginResponse>;
