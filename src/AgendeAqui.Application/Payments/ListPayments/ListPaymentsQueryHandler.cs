using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Common;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.Payments.ListPayments;

public sealed class ListPaymentsQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider) : IQueryHandler<ListPaymentsQuery, PagedResponse<PaymentResponse>>
{
    public async ValueTask<Result<PagedResponse<PaymentResponse>>> Handle(
        ListPaymentsQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        var tenantId = tenantProvider.GetTenantId();
        var offset = (query.Page - 1) * query.PageSize;

        const string countSql = """
            SELECT CAST(COUNT(*) AS integer)
            FROM payments p
            WHERE p.tenant_id = @TenantId
                AND (@AppointmentId IS NULL OR p.appointment_id = @AppointmentId)
                AND (@From IS NULL OR p.created_at >= @From)
                AND (@To IS NULL OR p.created_at < @ToExclusive)
            """;

        const string dataSql = """
            SELECT p.id          AS Id,
                   p.appointment_id AS AppointmentId,
                   COALESCE(c.name, '') AS ClientName,
                   COALESCE(s.name, '') AS ServiceName,
                   p.amount      AS Amount,
                   p.method      AS Method,
                   p.status      AS Status,
                   p.notes       AS Notes,
                   p.created_at  AS CreatedAt
            FROM payments p
            LEFT JOIN appointments a ON a.id = p.appointment_id
            LEFT JOIN clients c ON c.id = a.client_id
            LEFT JOIN services s ON s.id = a.service_id
            WHERE p.tenant_id = @TenantId
                AND (@AppointmentId IS NULL OR p.appointment_id = @AppointmentId)
                AND (@From IS NULL OR p.created_at >= @From)
                AND (@To IS NULL OR p.created_at < @ToExclusive)
            ORDER BY p.created_at DESC
            LIMIT @PageSize OFFSET @Offset
            """;

        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);
        parameters.Add("AppointmentId", query.AppointmentId);
        parameters.Add("From", query.From?.ToDateTime(TimeOnly.MinValue), System.Data.DbType.DateTime);
        parameters.Add("ToExclusive", query.To?.ToDateTime(TimeOnly.MinValue).AddDays(1), System.Data.DbType.DateTime);
        parameters.Add("PageSize", query.PageSize);
        parameters.Add("Offset", offset);

        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken));

        var items = await connection.QueryAsync<PaymentResponse>(
            new CommandDefinition(dataSql, parameters, cancellationToken: cancellationToken));

        var response = new PagedResponse<PaymentResponse>(
            items.AsList(),
            query.Page,
            query.PageSize,
            totalCount);

        return Result.Success(response);
    }
}
