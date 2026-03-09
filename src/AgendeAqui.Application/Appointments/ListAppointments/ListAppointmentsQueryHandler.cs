using System.Text;
using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Appointments.GetAppointment;
using AgendeAqui.Application.Common;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.Appointments.ListAppointments;

public sealed class ListAppointmentsQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider) : IQueryHandler<ListAppointmentsQuery, PagedResponse<AppointmentResponse>>
{
    public async ValueTask<Result<PagedResponse<AppointmentResponse>>> Handle(
        ListAppointmentsQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        var tenantId = tenantProvider.GetTenantId();
        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);

        var whereClause = new StringBuilder("WHERE a.tenant_id = @TenantId");

        if (query.DateFrom.HasValue)
        {
            whereClause.Append(" AND a.date >= @DateFrom");
            parameters.Add("DateFrom", query.DateFrom.Value);
        }

        if (query.DateTo.HasValue)
        {
            whereClause.Append(" AND a.date <= @DateTo");
            parameters.Add("DateTo", query.DateTo.Value);
        }

        if (query.ProfessionalId.HasValue)
        {
            whereClause.Append(" AND a.professional_id = @ProfessionalId");
            parameters.Add("ProfessionalId", query.ProfessionalId.Value);
        }

        if (query.ClientId.HasValue)
        {
            whereClause.Append(" AND a.client_id = @ClientId");
            parameters.Add("ClientId", query.ClientId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.ExternalId))
        {
            whereClause.Append(" AND a.external_id = @ExternalId");
            parameters.Add("ExternalId", query.ExternalId);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            whereClause.Append(" AND a.status = @Status");
            parameters.Add("Status", query.Status);
        }

        var where = whereClause.ToString();

        var countSql = $"SELECT COUNT(*) FROM appointments a {where}";
        var countCommand = new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken);
        var totalCount = await connection.ExecuteScalarAsync<int>(countCommand);

        var offset = (query.Page - 1) * query.PageSize;
        parameters.Add("PageSize", query.PageSize);
        parameters.Add("Offset", offset);

        var itemsSql = $"""
            SELECT a.id              AS Id,
                   a.professional_id AS ProfessionalId,
                   p.name            AS ProfessionalName,
                   a.service_id      AS ServiceId,
                   s.name            AS ServiceName,
                   a.client_id       AS ClientId,
                   c.name            AS ClientName,
                   a.date            AS Date,
                   a.start_time      AS StartTime,
                   a.end_time        AS EndTime,
                   a.status          AS Status,
                   a.notes           AS Notes,
                   a.created_at      AS CreatedAt
            FROM appointments a
            INNER JOIN professionals p ON p.id = a.professional_id AND p.tenant_id = @TenantId
            INNER JOIN services s ON s.id = a.service_id AND s.tenant_id = @TenantId
            INNER JOIN clients c ON c.id = a.client_id AND c.tenant_id = @TenantId
            {where}
            ORDER BY a.date, a.start_time
            LIMIT @PageSize OFFSET @Offset
            """;

        var itemsCommand = new CommandDefinition(itemsSql, parameters, cancellationToken: cancellationToken);
        var items = await connection.QueryAsync<AppointmentResponse>(itemsCommand);

        var response = new PagedResponse<AppointmentResponse>(
            items.AsList(),
            query.Page,
            query.PageSize,
            totalCount);

        return Result.Success(response);
    }
}
