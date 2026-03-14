using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Common;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.Appointments.ListMyAppointments;

public sealed class ListMyAppointmentsQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider,
    ICurrentUser currentUser) : IQueryHandler<ListMyAppointmentsQuery, PagedResponse<MyAppointmentResponse>>
{
    public async ValueTask<Result<PagedResponse<MyAppointmentResponse>>> Handle(
        ListMyAppointmentsQuery query,
        CancellationToken cancellationToken)
    {
        if (currentUser.ClientId is null)
            return Result.Failure<PagedResponse<MyAppointmentResponse>>(AppointmentErrors.NotClientProfile);

        var tenantId = tenantProvider.GetTenantId();
        var clientId = currentUser.ClientId.Value;

        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);
        parameters.Add("ClientId", clientId);

        var statusFilter = "";
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            statusFilter = " AND a.status = @Status";
            parameters.Add("Status", query.Status);
        }

        var countSql = $"""
            SELECT COUNT(*)
            FROM appointments a
            WHERE a.tenant_id = @TenantId AND a.client_id = @ClientId{statusFilter}
            """;

        var countCommand = new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken);
        var totalCount = await connection.ExecuteScalarAsync<int>(countCommand);

        var offset = (query.Page - 1) * query.PageSize;
        parameters.Add("PageSize", query.PageSize);
        parameters.Add("Offset", offset);

        var itemsSql = $"""
            SELECT a.id         AS Id,
                   p.name       AS ProfessionalName,
                   s.name       AS ServiceName,
                   a.date       AS Date,
                   a.start_time AS StartTime,
                   a.end_time   AS EndTime,
                   a.status     AS Status,
                   a.notes      AS Notes
            FROM appointments a
            INNER JOIN professionals p ON p.id = a.professional_id AND p.tenant_id = @TenantId
            INNER JOIN services s ON s.id = a.service_id AND s.tenant_id = @TenantId
            WHERE a.tenant_id = @TenantId AND a.client_id = @ClientId{statusFilter}
            ORDER BY a.date DESC, a.start_time DESC
            LIMIT @PageSize OFFSET @Offset
            """;

        var itemsCommand = new CommandDefinition(itemsSql, parameters, cancellationToken: cancellationToken);
        var items = await connection.QueryAsync<MyAppointmentResponse>(itemsCommand);

        var response = new PagedResponse<MyAppointmentResponse>(
            items.AsList(),
            query.Page,
            query.PageSize,
            totalCount);

        return Result.Success(response);
    }
}
