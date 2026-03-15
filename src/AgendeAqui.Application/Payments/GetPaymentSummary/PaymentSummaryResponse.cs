namespace AgendeAqui.Application.Payments.GetPaymentSummary;

public sealed record PaymentSummaryResponse(
    decimal TotalReceived,
    decimal TotalPending,
    decimal TotalRefunded,
    int PaymentCount,
    List<MethodSummary> ByMethod);

public sealed record MethodSummary(
    string Method,
    decimal Total,
    int Count);
