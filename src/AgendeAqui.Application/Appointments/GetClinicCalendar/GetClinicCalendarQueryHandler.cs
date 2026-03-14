using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.Appointments.GetClinicCalendar;

public sealed class GetClinicCalendarQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider) : IQueryHandler<GetClinicCalendarQuery, ClinicCalendarResponse>
{
    public async ValueTask<Result<ClinicCalendarResponse>> Handle(
        GetClinicCalendarQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        var tenantId = tenantProvider.GetTenantId();

        const string sql = """
            SELECT
                a.id              AS Id,
                a.professional_id AS ProfessionalId,
                p.name            AS ProfessionalName,
                c.name            AS ClientName,
                s.name            AS ServiceName,
                a.start_time      AS StartTime,
                a.end_time        AS EndTime,
                a.status          AS Status
            FROM appointments a
            INNER JOIN professionals p ON p.id = a.professional_id
            INNER JOIN clients       c ON c.id = a.client_id
            INNER JOIN services      s ON s.id = a.service_id
            WHERE a.tenant_id = @TenantId
              AND a.date      = @Date
            ORDER BY p.name, a.start_time
            """;

        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);
        parameters.Add("Date", query.Date.ToDateTime(TimeOnly.MinValue), System.Data.DbType.Date);

        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<CalendarRow>(command);

        var professionals = rows
            .GroupBy(r => r.ProfessionalId)
            .Select(g => new ProfessionalDayResponse(
                g.Key,
                g.First().ProfessionalName,
                g.Select(r => new ClinicAppointmentResponse(
                    r.Id,
                    r.ClientName,
                    r.ServiceName,
                    r.StartTime.ToString(@"hh\:mm"),
                    r.EndTime.ToString(@"hh\:mm"),
                    r.Status))
                .ToList()))
            .ToList();

        return Result.Success(new ClinicCalendarResponse(query.Date, professionals));
    }

    private sealed record CalendarRow(
        Guid Id,
        Guid ProfessionalId,
        string ProfessionalName,
        string ClientName,
        string ServiceName,
        TimeSpan StartTime,
        TimeSpan EndTime,
        string Status);
}
