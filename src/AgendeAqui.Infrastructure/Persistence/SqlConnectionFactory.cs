using System.Data;
using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Domain.Abstractions;
using Npgsql;

namespace AgendeAqui.Infrastructure.Persistence;

internal sealed class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;
    private readonly ITenantProvider _tenantProvider;

    public SqlConnectionFactory(string connectionString, ITenantProvider tenantProvider)
    {
        _connectionString = connectionString;
        _tenantProvider = tenantProvider;
    }

    public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var tenantId = _tenantProvider.GetTenantId();
        if (tenantId != Guid.Empty)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT set_config('app.current_tenant_id', @tenantId, false)";
            command.Parameters.Add(new NpgsqlParameter("tenantId", tenantId.ToString()));
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        return connection;
    }
}
