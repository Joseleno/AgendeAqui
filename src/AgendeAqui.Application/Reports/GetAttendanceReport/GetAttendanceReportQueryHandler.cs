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
                   CAST(COUNT(*) FILTER (WHERE a.status IN ('Completed','Cancelled','NoShow')) AS integer) AS Total,
                   CAST(COUNT(*) FILTER (WHERE a.status = 'Completed') AS integer) AS Completed,
                   CAST(COUNT(*) FILTER (WHERE a.status = 'Cancelled') AS integer) AS Cancelled,
                   CAST(COUNT(*) FILTER (WHERE a.status = 'NoShow') AS integer)    AS NoShow
            FROM appointments a
            INNER JOIN professionals p ON p.id = a.professional_id AND p.tenant_id = @TenantId
            WHERE a.tenant_id = @TenantId AND a.date BETWEEN @From AND @To
                AND (@ProfessionalId IS NULL OR a.professional_id = @ProfessionalId)
            GROUP BY p.id, p.name
            ORDER BY p.name
            """;

        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);
        parameters.Add("From", query.From.ToDateTime(TimeOnly.MinValue), System.Data.DbType.Date);
        parameters.Add("To", query.To.ToDateTime(TimeOnly.MinValue), System.Data.DbType.Date);
        parameters.Add("ProfessionalId", query.ProfessionalId);

        var command = new CommandDefinition(
            sql,
            parameters,
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
