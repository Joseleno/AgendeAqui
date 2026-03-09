using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Appointments.GetAppointment;
using AgendeAqui.Application.Common;

namespace AgendeAqui.Application.Appointments.ListAppointments;

public sealed record ListAppointmentsQuery : PagedRequest, IQuery<PagedResponse<AppointmentResponse>>
{
    public DateOnly? DateFrom { get; init; }
    public DateOnly? DateTo { get; init; }
    public Guid? ProfessionalId { get; init; }
    public Guid? ClientId { get; init; }
    public string? Status { get; init; }
    public string? ExternalId { get; init; }

    public ListAppointmentsQuery(
        int page,
        int pageSize,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        Guid? professionalId = null,
        string? status = null,
        Guid? clientId = null,
        string? externalId = null)
    {
        Page = page;
        PageSize = pageSize;
        DateFrom = dateFrom;
        DateTo = dateTo;
        ProfessionalId = professionalId;
        Status = status;
        ClientId = clientId;
        ExternalId = externalId;
    }
}
