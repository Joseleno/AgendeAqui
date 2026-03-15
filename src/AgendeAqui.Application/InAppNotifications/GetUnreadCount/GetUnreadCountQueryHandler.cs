using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.InAppNotifications.GetUnreadCount;

public sealed class GetUnreadCountQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider,
    ICurrentUser currentUser) : IQueryHandler<GetUnreadCountQuery, int>
{
    public async ValueTask<Result<int>> Handle(
        GetUnreadCountQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        var tenantId = tenantProvider.GetTenantId();
        var userId = currentUser.UserId;

        const string sql = "SELECT COUNT(*) FROM in_app_notifications WHERE tenant_id = @TenantId AND user_id = @UserId AND is_read = false";

        var command = new CommandDefinition(sql, new { TenantId = tenantId, UserId = userId }, cancellationToken: cancellationToken);
        var count = await connection.ExecuteScalarAsync<int>(command);

        return Result.Success(count);
    }
}
