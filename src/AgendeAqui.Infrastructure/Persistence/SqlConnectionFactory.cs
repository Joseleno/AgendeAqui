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

    public IDbConnection CreateConnection()
    {
        var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var tenantId = _tenantProvider.GetTenantId();
        if (tenantId != Guid.Empty)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"SET app.current_tenant_id = '{tenantId}'";
            command.ExecuteNonQuery();
        }

        return connection;
    }
}
