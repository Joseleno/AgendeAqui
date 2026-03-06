using System.Data.Common;
using AgendeAqui.Domain.Abstractions;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AgendeAqui.Infrastructure.Persistence.Interceptors;

internal sealed class TenantInterceptor : DbCommandInterceptor
{
    private readonly ITenantProvider _tenantProvider;

    public TenantInterceptor(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
    }

    public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        await SetTenantContextAsync(command, cancellationToken);
        return result;
    }

    public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        await SetTenantContextAsync(command, cancellationToken);
        return result;
    }

    public override async ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        await SetTenantContextAsync(command, cancellationToken);
        return result;
    }

    private async Task SetTenantContextAsync(DbCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();
        if (tenantId == Guid.Empty)
            return;

        using var setTenantCommand = command.Connection!.CreateCommand();
        setTenantCommand.Transaction = command.Transaction;
        setTenantCommand.CommandText = $"SET app.current_tenant_id = '{tenantId}'";
        await setTenantCommand.ExecuteNonQueryAsync(cancellationToken);
    }
}
