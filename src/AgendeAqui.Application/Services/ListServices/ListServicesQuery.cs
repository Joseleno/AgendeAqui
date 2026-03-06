using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Common;
using AgendeAqui.Application.Services.GetService;

namespace AgendeAqui.Application.Services.ListServices;

public sealed record ListServicesQuery(int Page, int PageSize) : IQuery<PagedResponse<ServiceResponse>>;
