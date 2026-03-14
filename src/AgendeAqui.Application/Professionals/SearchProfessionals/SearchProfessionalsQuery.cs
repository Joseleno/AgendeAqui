using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Common;
using AgendeAqui.Application.Professionals.GetProfessional;

namespace AgendeAqui.Application.Professionals.SearchProfessionals;

public sealed record SearchProfessionalsQuery(
    string? Name = null,
    string? Specialty = null,
    Guid? ServiceId = null,
    int Page = 1,
    int PageSize = 10) : IQuery<PagedResponse<ProfessionalResponse>>;
