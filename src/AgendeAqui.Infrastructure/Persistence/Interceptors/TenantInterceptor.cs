using System.Data.Common;
using AgendeAqui.Domain.Abstractions;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AgendeAqui.Infrastructure.Persistence.Interceptors;

internal sealed class TenantInterceptor : DbConnectionInterceptor
{
    private readonly ITenantProvider _tenantProvider;

    public TenantInterceptor(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
    }

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        SetTenantContext(connection);
        base.ConnectionOpened(connection, eventData);
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        await SetTenantContextAsync(connection, cancellationToken);
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    private void SetTenantContext(DbConnection connection)
    {
        var tenantId = _tenantProvider.GetTenantId();
        if (tenantId == Guid.Empty)
            return;

        using var command = CreateSetTenantCommand(connection, tenantId);
        command.ExecuteNonQuery();
    }

    private async Task SetTenantContextAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();
        if (tenantId == Guid.Empty)
            return;

        using var command = CreateSetTenantCommand(connection, tenantId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static DbCommand CreateSetTenantCommand(DbConnection connection, Guid tenantId)
    {
        var command = connection.CreateCommand();
        command.CommandText = "SELECT set_config('app.current_tenant_id', @tenantId, false)";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "tenantId";
        parameter.Value = tenantId.ToString();
        command.Parameters.Add(parameter);
        return command;
    }
}
