using System.Data;

namespace AgendeAqui.Application.Abstractions.Data;

// Produces connections that bypass RLS — for platform-operator cross-tenant queries only.
public interface IPlatformSqlConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
}
