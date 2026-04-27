using AgendeAqui.Application.Abstractions;
using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Tenants;
using Dapper;

namespace AgendeAqui.Infrastructure;

internal sealed class PlanLimitChecker(
    ITenantRepository tenantRepository,
    IPlatformSqlConnectionFactory connectionFactory) : IPlanLimitChecker
{
    private static readonly Error ProfessionalLimitReached =
        new("Plan.ProfessionalLimitReached", "Your plan does not allow more professionals. Upgrade to add more.");

    private static readonly Error ClientLimitReached =
        new("Plan.ClientLimitReached", "Your plan does not allow more clients. Upgrade to add more.");

    private static readonly Error MonthlyAppointmentLimitReached =
        new("Plan.MonthlyAppointmentLimitReached", "Monthly appointment limit reached for your plan. Upgrade to continue.");

    public async Task<Result> CheckProfessionalLimitAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await tenantRepository.GetByIdAsync(tenantId, ct);
        if (tenant is null) return Result.Success();

        var limits = PlanLimits.For(tenant.Plan);
        if (limits.MaxProfessionals == int.MaxValue) return Result.Success();

        using var conn = await connectionFactory.CreateConnectionAsync(ct);
        var count = await conn.QuerySingleAsync<int>(
            new CommandDefinition(
                "SELECT COUNT(*) FROM professionals WHERE tenant_id = @TenantId AND is_active = true",
                new { TenantId = tenantId },
                cancellationToken: ct));

        return count >= limits.MaxProfessionals
            ? Result.Failure(ProfessionalLimitReached)
            : Result.Success();
    }

    public async Task<Result> CheckClientLimitAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await tenantRepository.GetByIdAsync(tenantId, ct);
        if (tenant is null) return Result.Success();

        var limits = PlanLimits.For(tenant.Plan);
        if (limits.MaxClients == int.MaxValue) return Result.Success();

        using var conn = await connectionFactory.CreateConnectionAsync(ct);
        var count = await conn.QuerySingleAsync<int>(
            new CommandDefinition(
                "SELECT COUNT(*) FROM clients WHERE tenant_id = @TenantId",
                new { TenantId = tenantId },
                cancellationToken: ct));

        return count >= limits.MaxClients
            ? Result.Failure(ClientLimitReached)
            : Result.Success();
    }

    public async Task<Result> CheckMonthlyAppointmentLimitAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await tenantRepository.GetByIdAsync(tenantId, ct);
        if (tenant is null) return Result.Success();

        var limits = PlanLimits.For(tenant.Plan);
        if (limits.MaxAppointmentsPerMonth == int.MaxValue) return Result.Success();

        using var conn = await connectionFactory.CreateConnectionAsync(ct);
        var count = await conn.QuerySingleAsync<int>(
            new CommandDefinition(
                """
                SELECT COUNT(*) FROM appointments
                WHERE tenant_id = @TenantId
                  AND date >= date_trunc('month', CURRENT_DATE)
                  AND date < date_trunc('month', CURRENT_DATE) + INTERVAL '1 month'
                """,
                new { TenantId = tenantId },
                cancellationToken: ct));

        return count >= limits.MaxAppointmentsPerMonth
            ? Result.Failure(MonthlyAppointmentLimitReached)
            : Result.Success();
    }
}
