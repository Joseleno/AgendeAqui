using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Common;

namespace AgendeAqui.Application.Professionals.ListMyPatients;

public sealed record ListMyPatientsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null) : IQuery<PagedResponse<PatientSummaryResponse>>;
