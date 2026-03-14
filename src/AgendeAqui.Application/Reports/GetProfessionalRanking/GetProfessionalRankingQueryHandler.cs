using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using Dapper;
using Microsoft.Extensions.Caching.Hybrid;

namespace AgendeAqui.Application.Reports.GetProfessionalRanking;

public sealed class GetProfessionalRankingQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider,
    HybridCache cache) : IQueryHandler<GetProfessionalRankingQuery, List<ProfessionalRankingResponse>>
{
    public async ValueTask<Result<List<ProfessionalRankingResponse>>> Handle(
        GetProfessionalRankingQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.GetTenantId();

        var response = await cache.GetOrCreateAsync(
            $"ranking:{tenantId}:{query.From}:{query.To}",
            async ct =>
            {
                using var connection = await sqlConnectionFactory.CreateConnectionAsync(ct);

                const string sql = """
                    SELECT
                        p.id        AS ProfessionalId,
                        p.name      AS Name,
                        p.specialty AS Specialty,
                        CAST(COUNT(a.id) AS integer) AS Total,
                        CAST(COUNT(a.id) FILTER (WHERE a.status = 'Completed') AS integer) AS Completed,
                        CAST(COUNT(a.id) FILTER (WHERE a.status = 'Cancelled') AS integer) AS Cancelled,
                        CAST(COUNT(a.id) FILTER (WHERE a.status = 'NoShow') AS integer)    AS NoShow
                    FROM professionals p
                    LEFT JOIN appointments a
                        ON a.professional_id = p.id
                       AND a.tenant_id = @TenantId
                       AND a.date BETWEEN @From AND @To
                    WHERE p.tenant_id = @TenantId
                      AND p.is_active = true
                    GROUP BY p.id, p.name, p.specialty
                    ORDER BY COUNT(a.id) FILTER (WHERE a.status = 'Completed') DESC
                    """;

                var parameters = new DynamicParameters();
                parameters.Add("TenantId", tenantId);
                parameters.Add("From", query.From.ToDateTime(TimeOnly.MinValue), System.Data.DbType.Date);
                parameters.Add("To", query.To.ToDateTime(TimeOnly.MinValue), System.Data.DbType.Date);

                var command = new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken: ct);

                var rows = await connection.QueryAsync<RankingRow>(command);

                return rows
                    .Select(r =>
                    {
                        var completionRate = r.Total > 0
                            ? Math.Round((decimal)r.Completed / r.Total * 100, 2)
                            : 0m;

                        var noShowRate = r.Total > 0
                            ? Math.Round((decimal)r.NoShow / r.Total * 100, 2)
                            : 0m;

                        return new ProfessionalRankingResponse(
                            ProfessionalId: r.ProfessionalId,
                            Name: r.Name,
                            Specialty: r.Specialty,
                            Total: r.Total,
                            Completed: r.Completed,
                            Cancelled: r.Cancelled,
                            NoShow: r.NoShow,
                            CompletionRate: completionRate,
                            NoShowRate: noShowRate);
                    })
                    .ToList();
            },
            tags: [$"tenant:{tenantId}", "reports"],
            cancellationToken: cancellationToken);

        return Result.Success(response!);
    }

    private sealed record RankingRow(
        Guid ProfessionalId,
        string Name,
        string? Specialty,
        int Total,
        int Completed,
        int Cancelled,
        int NoShow);
}
