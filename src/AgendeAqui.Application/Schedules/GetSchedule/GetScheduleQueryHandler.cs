using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Schedules;
using Dapper;

namespace AgendeAqui.Application.Schedules.GetSchedule;

public sealed class GetScheduleQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider) : IQueryHandler<GetScheduleQuery, ScheduleResponse>
{
    public async ValueTask<Result<ScheduleResponse>> Handle(
        GetScheduleQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        const string sql = """
            SELECT s.id                                       AS Id,
                   s.professional_id                         AS ProfessionalId,
                   p.name                                    AS ProfessionalName,
                   s.day_of_week                             AS DayOfWeek,
                   s.start_time                              AS StartTime,
                   s.end_time                                AS EndTime,
                   CAST(EXTRACT(EPOCH FROM s.slot_duration) / 60 AS integer) AS SlotDurationMinutes,
                   s.is_active                               AS IsActive,
                   s.created_at                              AS CreatedAt
            FROM schedules s
            INNER JOIN professionals p ON p.id = s.professional_id AND p.tenant_id = @TenantId
            WHERE s.id = @ScheduleId AND s.tenant_id = @TenantId
            """;

        var command = new CommandDefinition(
            sql,
            new { query.ScheduleId, TenantId = tenantProvider.GetTenantId() },
            cancellationToken: cancellationToken);

        var schedule = await connection.QueryFirstOrDefaultAsync<ScheduleResponse>(command);

        if (schedule is null)
            return Result.Failure<ScheduleResponse>(ScheduleErrors.NotFound);

        return Result.Success(schedule);
    }
}
