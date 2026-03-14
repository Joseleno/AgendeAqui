using System.Text;
using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.Reports.ExportAppointmentsCsv;

public sealed class ExportAppointmentsCsvQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider) : IQueryHandler<ExportAppointmentsCsvQuery, string>
{
    public async ValueTask<Result<string>> Handle(
        ExportAppointmentsCsvQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        var tenantId = tenantProvider.GetTenantId();

        const string sql = """
            SELECT
                a.date        AS Date,
                a.start_time  AS StartTime,
                a.end_time    AS EndTime,
                a.status      AS Status,
                a.notes       AS Notes,
                p.name        AS ProfessionalName,
                p.specialty   AS Specialty,
                c.name        AS ClientName,
                c.email       AS ClientEmail,
                c.phone       AS ClientPhone,
                s.name        AS ServiceName
            FROM appointments a
            INNER JOIN professionals p ON p.id = a.professional_id
            INNER JOIN clients c ON c.id = a.client_id
            INNER JOIN services s ON s.id = a.service_id
            WHERE a.tenant_id = @TenantId
                AND a.date BETWEEN @From AND @To
                AND (@ProfessionalId IS NULL OR a.professional_id = @ProfessionalId)
            ORDER BY a.date, a.start_time
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

        var rows = await connection.QueryAsync<AppointmentRow>(command);

        var csv = BuildCsv(rows);

        return Result.Success(csv);
    }

    private static string BuildCsv(IEnumerable<AppointmentRow> rows)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Data;Horario Inicio;Horario Fim;Profissional;Especialidade;Cliente;Email;Telefone;Servico;Status;Observacoes");

        foreach (var row in rows)
        {
            sb.Append(row.Date.ToString("dd/MM/yyyy"));
            sb.Append(';');
            sb.Append(FormatTime(row.StartTime));
            sb.Append(';');
            sb.Append(FormatTime(row.EndTime));
            sb.Append(';');
            sb.Append(EscapeCsvField(row.ProfessionalName));
            sb.Append(';');
            sb.Append(EscapeCsvField(row.Specialty));
            sb.Append(';');
            sb.Append(EscapeCsvField(row.ClientName));
            sb.Append(';');
            sb.Append(EscapeCsvField(MaskEmail(row.ClientEmail)));
            sb.Append(';');
            sb.Append(EscapeCsvField(MaskPhone(row.ClientPhone)));
            sb.Append(';');
            sb.Append(EscapeCsvField(row.ServiceName));
            sb.Append(';');
            sb.Append(EscapeCsvField(row.Status));
            sb.Append(';');
            sb.Append(EscapeCsvField(row.Notes));
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string FormatTime(TimeSpan time) =>
        $"{(int)time.TotalHours:D2}:{time.Minutes:D2}";

    private static string MaskEmail(string? email)
    {
        if (string.IsNullOrEmpty(email))
            return string.Empty;

        var atIndex = email.IndexOf('@');
        if (atIndex <= 1)
            return email;

        return $"{email[0]}***{email[(atIndex - 1)..]}";
    }

    private static string MaskPhone(string? phone)
    {
        if (string.IsNullOrEmpty(phone) || phone.Length < 4)
            return string.Empty;

        return $"{new string('*', phone.Length - 4)}{phone[^4..]}";
    }

    private static string EscapeCsvField(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.StartsWith('=') || value.StartsWith('+') || value.StartsWith('-') ||
            value.StartsWith('@') || value.StartsWith('|'))
            value = $"'{value}";

        return value.Contains(';') || value.Contains('"') ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
    }

    private sealed record AppointmentRow(
        DateOnly Date,
        TimeSpan StartTime,
        TimeSpan EndTime,
        string Status,
        string? Notes,
        string ProfessionalName,
        string? Specialty,
        string ClientName,
        string ClientEmail,
        string ClientPhone,
        string ServiceName);
}
