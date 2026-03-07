using System.Text;
using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Common;
using AgendeAqui.Application.Schedules.GetSchedule;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.Schedules.ListSchedules;

public sealed class ListSchedulesQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider) : IQueryHandler<ListSchedulesQuery, PagedResponse<ScheduleResponse>>
{
    public async ValueTask<Result<PagedResponse<ScheduleResponse>>> Handle(
        ListSchedulesQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        var tenantId = tenantProvider.GetTenantId();
        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);

        var whereClause = new StringBuilder("WHERE s.tenant_id = @TenantId");

        if (query.ProfessionalId.HasValue)
        {
            whereClause.Append(" AND s.professional_id = @ProfessionalId");
            parameters.Add("ProfessionalId", query.ProfessionalId.Value);
        }

        var where = whereClause.ToString();

        var countSql = $"""
            SELECT COUNT(*)
            FROM schedules s
            INNER JOIN professionals p ON p.id = s.professional_id AND p.tenant_id = @TenantId
            {where}
            """;
        var countCommand = new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken);
        var totalCount = await connection.ExecuteScalarAsync<int>(countCommand);

        var offset = (query.Page - 1) * query.PageSize;
        parameters.Add("PageSize", query.PageSize);
        parameters.Add("Offset", offset);

        var itemsSql = $"""
            SELECT s.id                                       AS Id,
                   s.professional_id                         AS ProfessionalId,
                   p.name                                    AS ProfessionalName,
                   s.day_of_week                             AS DayOfWeek,
                   s.start_time                              AS StartTime,
                   s.end_time                                AS EndTime,
                   EXTRACT(EPOCH FROM s.slot_duration) / 60  AS SlotDurationMinutes,
                   s.is_active                               AS IsActive,
                   s.created_at                              AS CreatedAt
            FROM schedules s
            INNER JOIN professionals p ON p.id = s.professional_id AND p.tenant_id = @TenantId
            {where}
            ORDER BY s.day_of_week, s.start_time
            LIMIT @PageSize OFFSET @Offset
            """;

        var itemsCommand = new CommandDefinition(itemsSql, parameters, cancellationToken: cancellationToken);
        var items = await connection.QueryAsync<ScheduleResponse>(itemsCommand);

        var response = new PagedResponse<ScheduleResponse>(
            items.AsList(),
            query.Page,
            query.PageSize,
            totalCount);

        return Result.Success(response);
    }
}
