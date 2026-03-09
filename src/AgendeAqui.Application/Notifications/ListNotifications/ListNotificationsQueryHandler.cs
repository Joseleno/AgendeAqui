using System.Text;
using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.Notifications.ListNotifications;

public sealed class ListNotificationsQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider) : IQueryHandler<ListNotificationsQuery, List<NotificationResponse>>
{
    public async ValueTask<Result<List<NotificationResponse>>> Handle(
        ListNotificationsQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        var tenantId = tenantProvider.GetTenantId();
        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);

        var whereClause = new StringBuilder("WHERE n.tenant_id = @TenantId");

        if (query.AppointmentId.HasValue)
        {
            whereClause.Append(" AND n.appointment_id = @AppointmentId");
            parameters.Add("AppointmentId", query.AppointmentId.Value);
        }

        if (query.DateFrom.HasValue)
        {
            whereClause.Append(" AND n.created_at >= @DateFrom");
            parameters.Add("DateFrom", query.DateFrom.Value.ToDateTime(TimeOnly.MinValue));
        }

        if (query.DateTo.HasValue)
        {
            whereClause.Append(" AND n.created_at < @DateTo");
            parameters.Add("DateTo", query.DateTo.Value.AddDays(1).ToDateTime(TimeOnly.MinValue));
        }

        var sql = $"""
            SELECT n.id AS Id,
                   n.appointment_id AS AppointmentId,
                   n.channel AS Channel,
                   n.recipient AS Recipient,
                   n.template_name AS TemplateName,
                   n.status AS Status,
                   n.sent_at AS SentAt,
                   n.error_message AS ErrorMessage,
                   n.created_at AS CreatedAt
            FROM notifications n
            {whereClause}
            ORDER BY n.created_at DESC
            """;

        var command = new CommandDefinition(
            sql,
            parameters,
            cancellationToken: cancellationToken);

        var notifications = await connection.QueryAsync<NotificationResponse>(command);

        return Result.Success(notifications.ToList());
    }
}
