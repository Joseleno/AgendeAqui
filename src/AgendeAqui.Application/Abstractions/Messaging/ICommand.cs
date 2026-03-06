using AgendeAqui.Domain.Common;
using Mediator;

namespace AgendeAqui.Application.Abstractions.Messaging;

public interface ICommand : ICommand<Unit>;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>;
