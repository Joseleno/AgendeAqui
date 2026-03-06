using AgendeAqui.Domain.Common;
using Mediator;

namespace AgendeAqui.Application.Abstractions.Messaging;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
