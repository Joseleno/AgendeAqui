namespace AgendeAqui.Api.Endpoints.Requests;

public sealed record CreatePaymentRequest(Guid AppointmentId, decimal Amount, string Method, string? Notes);

public sealed record UpdatePaymentStatusRequest(string Status);
