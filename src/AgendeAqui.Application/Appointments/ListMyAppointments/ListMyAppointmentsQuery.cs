using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Common;

namespace AgendeAqui.Application.Appointments.ListMyAppointments;

public sealed record ListMyAppointmentsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Status = null) : IQuery<PagedResponse<MyAppointmentResponse>>;
