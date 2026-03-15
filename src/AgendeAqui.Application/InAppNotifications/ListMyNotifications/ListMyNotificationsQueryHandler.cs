using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Common;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.InAppNotifications.ListMyNotifications;

public sealed class ListMyNotificationsQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider,
    ICurrentUser currentUser) : IQueryHandler<ListMyNotificationsQuery, PagedResponse<InAppNotificationResponse>>
{
    public async ValueTask<Result<PagedResponse<InAppNotificationResponse>>> Handle(
        ListMyNotificationsQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        var tenantId = tenantProvider.GetTenantId();
        var userId = currentUser.UserId;
        var offset = (query.Page - 1) * query.PageSize;

        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);
        parameters.Add("UserId", userId);
        parameters.Add("PageSize", query.PageSize);
        parameters.Add("Offset", offset);

        const string countSql = "SELECT COUNT(*) FROM in_app_notifications WHERE tenant_id = @TenantId AND user_id = @UserId";
        var countCommand = new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken);
        var totalCount = await connection.ExecuteScalarAsync<int>(countCommand);

        const string itemsSql = """
            SELECT id          AS Id,
                   title       AS Title,
                   message     AS Message,
                   type        AS Type,
                   reference_id AS ReferenceId,
                   is_read     AS IsRead,
                   created_at  AS CreatedAt
            FROM in_app_notifications
            WHERE tenant_id = @TenantId AND user_id = @UserId
            ORDER BY created_at DESC
            LIMIT @PageSize OFFSET @Offset
            """;

        var itemsCommand = new CommandDefinition(itemsSql, parameters, cancellationToken: cancellationToken);
        var items = await connection.QueryAsync<InAppNotificationResponse>(itemsCommand);

        var response = new PagedResponse<InAppNotificationResponse>(
            items.AsList(),
            query.Page,
            query.PageSize,
            totalCount);

        return Result.Success(response);
    }
}
