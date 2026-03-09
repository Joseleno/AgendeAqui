using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.Reports.GetAttendanceReport;

public sealed class GetAttendanceReportQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider) : IQueryHandler<GetAttendanceReportQuery, AttendanceReportResponse>
{
    public async ValueTask<Result<AttendanceReportResponse>> Handle(
        GetAttendanceReportQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        var tenantId = tenantProvider.GetTenantId();

        const string sql = """
            SELECT p.id   AS ProfessionalId,
                   p.name AS ProfessionalName,
                   COUNT(*) FILTER (WHERE a.status IN ('Completed','Cancelled','NoShow')) AS Total,
                   COUNT(*) FILTER (WHERE a.status = 'Completed') AS Completed,
                   COUNT(*) FILTER (WHERE a.status = 'Cancelled') AS Cancelled,
                   COUNT(*) FILTER (WHERE a.status = 'NoShow')    AS NoShow
            FROM appointments a
            INNER JOIN professionals p ON p.id = a.professional_id AND p.tenant_id = @TenantId
            WHERE a.tenant_id = @TenantId AND a.date BETWEEN @From AND @To
            GROUP BY p.id, p.name
            ORDER BY p.name
            """;

        var command = new CommandDefinition(
            sql,
            new { TenantId = tenantId, query.From, query.To },
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<ProfessionalAttendance>(command);
        var breakdown = rows.AsList();

        var response = new AttendanceReportResponse(
            TotalAppointments: breakdown.Sum(r => r.Total),
            TotalCompleted: breakdown.Sum(r => r.Completed),
            TotalCancelled: breakdown.Sum(r => r.Cancelled),
            TotalNoShow: breakdown.Sum(r => r.NoShow),
            Breakdown: breakdown);

        return Result.Success(response);
    }
}
