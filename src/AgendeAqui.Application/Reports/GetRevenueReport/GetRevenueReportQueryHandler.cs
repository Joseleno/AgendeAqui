using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.Reports.GetRevenueReport;

public sealed class GetRevenueReportQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider) : IQueryHandler<GetRevenueReportQuery, RevenueReportResponse>
{
    public async ValueTask<Result<RevenueReportResponse>> Handle(
        GetRevenueReportQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        var tenantId = tenantProvider.GetTenantId();

        const string sql = """
            SELECT s.id    AS ServiceId,
                   s.name  AS ServiceName,
                   s.price AS UnitPrice,
                   COUNT(*) AS AppointmentCount,
                   SUM(s.price) AS TotalRevenue
            FROM appointments a
            INNER JOIN services s ON s.id = a.service_id AND s.tenant_id = @TenantId
            WHERE a.tenant_id = @TenantId AND a.date BETWEEN @From AND @To AND a.status = 'Completed'
            GROUP BY s.id, s.name, s.price
            ORDER BY TotalRevenue DESC
            """;

        var command = new CommandDefinition(
            sql,
            new { TenantId = tenantId, query.From, query.To },
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<ServiceRevenue>(command);
        var byService = rows.AsList();

        var response = new RevenueReportResponse(
            TotalRevenue: byService.Sum(r => r.TotalRevenue),
            ByService: byService);

        return Result.Success(response);
    }
}
