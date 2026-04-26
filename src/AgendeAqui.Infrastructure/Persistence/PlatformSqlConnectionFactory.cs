using System.Data;
using AgendeAqui.Application.Abstractions.Data;
using Npgsql;

namespace AgendeAqui.Infrastructure.Persistence;

// Opens a raw connection without setting app.current_tenant_id — bypasses PostgreSQL RLS.
// ONLY inject this into platform-operator handlers. Never share with tenant-scoped code.
internal sealed class PlatformSqlConnectionFactory : IPlatformSqlConnectionFactory
{
    private readonly string _connectionString;

    public PlatformSqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
