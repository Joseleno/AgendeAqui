using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Payments.UpdatePaymentStatus;

public sealed record UpdatePaymentStatusCommand(
    Guid Id,
    string Status) : ICommand;
