namespace AgendeAqui.Application.Payments.ListPayments;

public sealed record PaymentResponse(
    Guid Id,
    Guid AppointmentId,
    string ClientName,
    string ServiceName,
    decimal Amount,
    string Method,
    string Status,
    string? Notes,
    DateTime CreatedAt);
