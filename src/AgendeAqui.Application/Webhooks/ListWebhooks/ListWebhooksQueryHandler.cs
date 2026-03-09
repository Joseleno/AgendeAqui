using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.Webhooks.ListWebhooks;

public sealed class ListWebhooksQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider) : IQueryHandler<ListWebhooksQuery, List<WebhookResponse>>
{
    public async ValueTask<Result<List<WebhookResponse>>> Handle(
        ListWebhooksQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        var tenantId = tenantProvider.GetTenantId();

        const string sql = """
            SELECT id        AS Id,
                   url       AS Url,
                   events    AS Events,
                   is_active AS IsActive,
                   created_at AS CreatedAt
            FROM webhooks
            WHERE tenant_id = @TenantId
            ORDER BY created_at DESC
            """;

        var command = new CommandDefinition(sql, new { TenantId = tenantId }, cancellationToken: cancellationToken);
        var rows = await connection.QueryAsync<WebhookRow>(command);
        var webhooks = rows.Select(r => r.ToResponse()).ToList();

        return Result.Success(webhooks);
    }
}
