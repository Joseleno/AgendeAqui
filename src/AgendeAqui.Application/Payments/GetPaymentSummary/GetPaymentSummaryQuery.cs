using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Payments.GetPaymentSummary;

public sealed record GetPaymentSummaryQuery(
    DateOnly From,
    DateOnly To) : IQuery<PaymentSummaryResponse>;
