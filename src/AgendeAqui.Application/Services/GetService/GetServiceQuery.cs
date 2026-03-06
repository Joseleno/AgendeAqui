using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Services.GetService;

public sealed record GetServiceQuery(Guid ServiceId) : IQuery<ServiceResponse>;
