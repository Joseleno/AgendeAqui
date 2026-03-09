using System.Data;

namespace AgendeAqui.Application.Abstractions.Data;

public interface ISqlConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
}
