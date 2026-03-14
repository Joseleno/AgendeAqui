using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.Appointments.GetMyCalendar;

public sealed class GetMyCalendarQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider,
    ICurrentUser currentUser) : IQueryHandler<GetMyCalendarQuery, CalendarResponse>
{
    public async ValueTask<Result<CalendarResponse>> Handle(
        GetMyCalendarQuery query,
        CancellationToken cancellationToken)
    {
        if (currentUser.ProfessionalId is null)
            return Result.Failure<CalendarResponse>(AppointmentErrors.NotAuthorized);

        var tenantId = tenantProvider.GetTenantId();
        var professionalId = currentUser.ProfessionalId.Value;

        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        var appointmentSql = """
            SELECT a.id         AS Id,
                   c.name       AS ClientName,
                   s.name       AS ServiceName,
                   a.date       AS Date,
                   a.start_time AS StartTime,
                   a.end_time   AS EndTime,
                   a.status     AS Status,
                   a.notes      AS Notes
            FROM appointments a
            INNER JOIN clients c ON c.id = a.client_id AND c.tenant_id = @TenantId
            INNER JOIN services s ON s.id = a.service_id AND s.tenant_id = @TenantId
            WHERE a.professional_id = @ProfessionalId
              AND a.tenant_id = @TenantId
              AND a.date BETWEEN @DateFrom AND @DateTo
            ORDER BY a.date, a.start_time
            """;

        var appointmentCommand = new CommandDefinition(
            appointmentSql,
            new { TenantId = tenantId, ProfessionalId = professionalId, query.DateFrom, query.DateTo },
            cancellationToken: cancellationToken);

        var appointmentRows = await connection.QueryAsync<AppointmentRow>(appointmentCommand);

        var absenceSql = """
            SELECT id         AS Id,
                   date       AS Date,
                   start_time AS StartTime,
                   end_time   AS EndTime,
                   reason     AS Reason
            FROM absences
            WHERE professional_id = @ProfessionalId
              AND tenant_id = @TenantId
              AND date BETWEEN @DateFrom AND @DateTo
            ORDER BY date
            """;

        var absenceCommand = new CommandDefinition(
            absenceSql,
            new { ProfessionalId = professionalId, TenantId = tenantId, query.DateFrom, query.DateTo },
            cancellationToken: cancellationToken);

        var absenceRows = await connection.QueryAsync<AbsenceRow>(absenceCommand);

        var appointmentsByDate = appointmentRows
            .GroupBy(r => r.Date)
            .ToDictionary(g => g.Key, g => g.ToList());

        var absencesByDate = absenceRows
            .GroupBy(r => r.Date)
            .ToDictionary(g => g.Key, g => g.ToList());

        var allDates = appointmentsByDate.Keys
            .Union(absencesByDate.Keys)
            .OrderBy(d => d)
            .ToList();

        var days = allDates.Select(date =>
        {
            var appointments = appointmentsByDate.TryGetValue(date, out var appts)
                ? appts.Select(a => new CalendarAppointmentResponse(
                    a.Id,
                    a.ClientName,
                    a.ServiceName,
                    a.StartTime.ToString(@"hh\:mm"),
                    a.EndTime.ToString(@"hh\:mm"),
                    a.Status,
                    a.Notes)).ToList()
                : [];

            var absences = absencesByDate.TryGetValue(date, out var abs)
                ? abs.Select(a => new CalendarAbsenceResponse(
                    a.Id,
                    a.StartTime?.ToString(@"hh\:mm"),
                    a.EndTime?.ToString(@"hh\:mm"),
                    a.StartTime is null && a.EndTime is null,
                    a.Reason)).ToList()
                : [];

            return new CalendarDayResponse(
                date,
                appointments,
                absences);
        }).ToList();

        return Result.Success(new CalendarResponse(days));
    }

    private sealed record AppointmentRow(
        Guid Id,
        string ClientName,
        string ServiceName,
        DateOnly Date,
        TimeSpan StartTime,
        TimeSpan EndTime,
        string Status,
        string? Notes);

    private sealed record AbsenceRow(
        Guid Id,
        DateOnly Date,
        TimeSpan? StartTime,
        TimeSpan? EndTime,
        string? Reason);
}
