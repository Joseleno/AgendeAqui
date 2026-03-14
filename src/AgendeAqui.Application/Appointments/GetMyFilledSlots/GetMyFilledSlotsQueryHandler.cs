using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.Appointments.GetMyFilledSlots;

public sealed class GetMyFilledSlotsQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    IScheduleRepository scheduleRepository,
    ITenantProvider tenantProvider,
    ICurrentUser currentUser) : IQueryHandler<GetMyFilledSlotsQuery, FilledSlotsResponse>
{
    public async ValueTask<Result<FilledSlotsResponse>> Handle(
        GetMyFilledSlotsQuery query,
        CancellationToken cancellationToken)
    {
        if (currentUser.ProfessionalId is null)
            return Result.Failure<FilledSlotsResponse>(AppointmentErrors.NotAuthorized);

        var tenantId = tenantProvider.GetTenantId();
        var professionalId = currentUser.ProfessionalId.Value;

        // Fetch the schedule for the day of week to determine total available slots.
        var schedule = await scheduleRepository.GetByProfessionalAndDayAsync(
            professionalId, query.Date.DayOfWeek, cancellationToken);

        // If no schedule exists, total slots is zero.
        var totalSlots = schedule is not null
            ? schedule.GetAvailableSlots(schedule.SlotDuration).Count
            : 0;

        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        var appointmentSql = """
            SELECT a.start_time AS StartTime,
                   a.end_time   AS EndTime,
                   c.name       AS ClientName,
                   s.name       AS ServiceName,
                   a.status     AS Status
            FROM appointments a
            INNER JOIN clients c ON c.id = a.client_id AND c.tenant_id = @TenantId
            INNER JOIN services s ON s.id = a.service_id AND s.tenant_id = @TenantId
            WHERE a.professional_id = @ProfessionalId
              AND a.tenant_id = @TenantId
              AND a.date = @Date
              AND a.status <> 'Cancelled'
            ORDER BY a.start_time
            """;

        var command = new CommandDefinition(
            appointmentSql,
            new { TenantId = tenantId, ProfessionalId = professionalId, query.Date },
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<AppointmentRow>(command);
        var appointmentList = rows.AsList();

        var filledSlots = appointmentList.Count;
        var availableSlots = Math.Max(0, totalSlots - filledSlots);

        var slots = appointmentList
            .Select(r => new FilledSlotResponse(
                r.StartTime.ToString(@"hh\:mm"),
                r.EndTime.ToString(@"hh\:mm"),
                r.ClientName,
                r.ServiceName,
                r.Status))
            .ToList();

        return Result.Success(new FilledSlotsResponse(
            query.Date,
            totalSlots,
            filledSlots,
            availableSlots,
            slots));
    }

    private sealed record AppointmentRow(
        TimeSpan StartTime,
        TimeSpan EndTime,
        string ClientName,
        string ServiceName,
        string Status);
}
