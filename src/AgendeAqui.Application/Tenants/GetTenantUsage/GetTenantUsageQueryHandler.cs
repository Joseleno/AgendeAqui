using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Tenants;
using Dapper;

namespace AgendeAqui.Application.Tenants.GetTenantUsage;

internal sealed class GetTenantUsageQueryHandler(
    ITenantRepository tenantRepository,
    IPlatformSqlConnectionFactory platformConnectionFactory) : IQueryHandler<GetTenantUsageQuery, TenantUsageResponse>
{
    public async ValueTask<Result<TenantUsageResponse>> Handle(
        GetTenantUsageQuery query,
        CancellationToken cancellationToken)
    {
        var tenant = await tenantRepository.GetByIdAsync(query.TenantId, cancellationToken);
        if (tenant is null)
            return Result.Failure<TenantUsageResponse>(TenantErrors.NotFound);

        using var connection = await platformConnectionFactory.CreateConnectionAsync(cancellationToken);

        const string usageSql = """
            SELECT
                CAST(COUNT(DISTINCT p.id) FILTER (WHERE p.is_active = true) AS integer) AS Professionals,
                CAST(COUNT(DISTINCT c.id) AS integer) AS Clients,
                CAST(COUNT(a.id) FILTER (
                    WHERE a.date >= date_trunc('month', CURRENT_DATE)
                      AND a.date < date_trunc('month', CURRENT_DATE) + INTERVAL '1 month'
                ) AS integer) AS AppointmentsThisMonth
            FROM tenants t
            LEFT JOIN professionals p ON p.tenant_id = t.id
            LEFT JOIN clients      c ON c.tenant_id = t.id
            LEFT JOIN appointments a ON a.tenant_id = t.id
            WHERE t.id = @TenantId
            """;

        var row = await connection.QuerySingleAsync<(int Professionals, int Clients, int AppointmentsThisMonth)>(
            new CommandDefinition(usageSql, new { TenantId = query.TenantId }, cancellationToken: cancellationToken));

        var limits = PlanLimits.For(tenant.Plan);

        return Result.Success(new TenantUsageResponse(
            TenantId: tenant.Id,
            Name: tenant.Name,
            Slug: tenant.Slug,
            Plan: tenant.Plan.ToString(),
            Status: tenant.Status.ToString(),
            IsOnTrial: tenant.IsOnTrial,
            TrialEndsAt: tenant.TrialEndsAt,
            CustomDomain: tenant.CustomDomain,
            ProfessionalsCount: row.Professionals,
            MaxProfessionals: limits.MaxProfessionals == int.MaxValue ? -1 : limits.MaxProfessionals,
            ClientsCount: row.Clients,
            MaxClients: limits.MaxClients == int.MaxValue ? -1 : limits.MaxClients,
            AppointmentsThisMonth: row.AppointmentsThisMonth,
            MaxAppointmentsPerMonth: limits.MaxAppointmentsPerMonth == int.MaxValue ? -1 : limits.MaxAppointmentsPerMonth));
    }
}
