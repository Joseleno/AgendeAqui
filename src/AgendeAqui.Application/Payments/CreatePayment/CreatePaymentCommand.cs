using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Payments.CreatePayment;

public sealed record CreatePaymentCommand(
    Guid AppointmentId,
    decimal Amount,
    string Method,
    string? Notes) : ICommand<Guid>;
