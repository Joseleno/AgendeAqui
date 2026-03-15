using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Common;

namespace AgendeAqui.Application.Payments.ListPayments;

public sealed record ListPaymentsQuery(
    Guid? AppointmentId,
    int Page = 1,
    int PageSize = 20,
    DateOnly? From = null,
    DateOnly? To = null) : IQuery<PagedResponse<PaymentResponse>>;
