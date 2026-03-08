using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Clients;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.Clients.ExportClientData;

public sealed class ExportClientDataQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider) : IQueryHandler<ExportClientDataQuery, ClientDataExport>
{
    public async ValueTask<Result<ClientDataExport>> Handle(
        ExportClientDataQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        var tenantId = tenantProvider.GetTenantId();

        const string sql = """
            SELECT c.id         AS ClientId,
                   c.name       AS Name,
                   c.email      AS Email,
                   c.phone      AS Phone,
                   c.created_at AS CreatedAt,
                   a.id         AS AppointmentId,
                   a.date       AS Date,
                   a.start_time AS StartTime,
                   a.end_time   AS EndTime,
                   a.status     AS Status,
                   a.created_at AS AppointmentCreatedAt
            FROM clients c
            LEFT JOIN appointments a ON a.client_id = c.id AND a.tenant_id = @TenantId
            WHERE c.id = @ClientId AND c.tenant_id = @TenantId
            ORDER BY a.date, a.start_time
            """;

        var command = new CommandDefinition(
            sql,
            new { query.ClientId, TenantId = tenantId },
            cancellationToken: cancellationToken);

        var rows = (await connection.QueryAsync<ClientExportRow>(command)).AsList();

        if (rows.Count == 0)
            return Result.Failure<ClientDataExport>(ClientErrors.NotFound);

        var first = rows[0];

        if (first.Name == "***ANONYMIZED***")
            return Result.Failure<ClientDataExport>(ClientErrors.AlreadyAnonymized);

        var appointments = rows
            .Where(r => r.AppointmentId.HasValue)
            .Select(r => new ClientAppointmentExport(
                r.AppointmentId!.Value,
                r.Date!.Value,
                r.StartTime!,
                r.EndTime!,
                r.Status!,
                r.AppointmentCreatedAt!.Value))
            .ToList();

        var export = new ClientDataExport(
            first.ClientId,
            first.Name,
            first.Email,
            first.Phone,
            first.CreatedAt,
            appointments);

        return Result.Success(export);
    }

    private sealed record ClientExportRow(
        Guid ClientId,
        string Name,
        string Email,
        string Phone,
        DateTime CreatedAt,
        Guid? AppointmentId,
        DateOnly? Date,
        string? StartTime,
        string? EndTime,
        string? Status,
        DateTime? AppointmentCreatedAt);
}
