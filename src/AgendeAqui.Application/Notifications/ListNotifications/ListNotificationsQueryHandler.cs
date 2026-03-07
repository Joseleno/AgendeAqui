using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.Notifications.ListNotifications;

public sealed class ListNotificationsQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory) : IQueryHandler<ListNotificationsQuery, List<NotificationResponse>>
{
    public async ValueTask<Result<List<NotificationResponse>>> Handle(
        ListNotificationsQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        const string sql = """
            SELECT id AS Id,
                   appointment_id AS AppointmentId,
                   channel AS Channel,
                   recipient AS Recipient,
                   template_name AS TemplateName,
                   status AS Status,
                   sent_at AS SentAt,
                   error_message AS ErrorMessage,
                   created_at AS CreatedAt
            FROM notifications
            WHERE appointment_id = @AppointmentId
            ORDER BY created_at DESC
            """;

        var command = new CommandDefinition(
            sql,
            new { query.AppointmentId },
            cancellationToken: cancellationToken);

        var notifications = await connection.QueryAsync<NotificationResponse>(command);

        return Result.Success(notifications.ToList());
    }
}
