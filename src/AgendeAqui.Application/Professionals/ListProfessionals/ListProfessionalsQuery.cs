using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Common;
using AgendeAqui.Application.Professionals.GetProfessional;

namespace AgendeAqui.Application.Professionals.ListProfessionals;

public sealed record ListProfessionalsQuery(int Page, int PageSize) : IQuery<PagedResponse<ProfessionalResponse>>;
