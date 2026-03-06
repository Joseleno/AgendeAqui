using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.Appointments.GetAppointment;

public sealed class GetAppointmentQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider) : IQueryHandler<GetAppointmentQuery, AppointmentResponse>
{
    public async ValueTask<Result<AppointmentResponse>> Handle(
        GetAppointmentQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        const string sql = """
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
            WHERE a.id = @AppointmentId AND a.tenant_id = @TenantId
            """;

        var command = new CommandDefinition(
            sql,
            new { query.AppointmentId, TenantId = tenantProvider.GetTenantId() },
            cancellationToken: cancellationToken);

        var appointment = await connection.QueryFirstOrDefaultAsync<AppointmentResponse>(command);

        if (appointment is null)
            return Result.Failure<AppointmentResponse>(AppointmentErrors.NotFound);

        return Result.Success(appointment);
    }
}
