using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.Teams.ListTeams;

internal sealed class ListTeamsQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider) : IQueryHandler<ListTeamsQuery, List<TeamResponse>>
{
    public async ValueTask<Result<List<TeamResponse>>> Handle(ListTeamsQuery query, CancellationToken cancellationToken)
    {
        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        const string sql = """
            SELECT t.id            AS Id,
                   t.name          AS Name,
                   t.description   AS Description,
                   t.leader_id     AS LeaderId,
                   p.name          AS LeaderName,
                   t.is_active     AS IsActive,
                   t.created_at    AS CreatedAt,
                   CAST(COUNT(tm.id) FILTER (WHERE tm.left_at IS NULL) AS integer) AS MemberCount
            FROM teams t
            LEFT JOIN professionals p ON p.id = t.leader_id AND p.tenant_id = @TenantId
            LEFT JOIN team_members tm ON tm.team_id = t.id AND tm.tenant_id = @TenantId AND tm.left_at IS NULL
            WHERE t.tenant_id = @TenantId
            GROUP BY t.id, t.name, t.description, t.leader_id, p.name, t.is_active, t.created_at
            ORDER BY t.name
            """;

        var rows = await connection.QueryAsync<TeamResponse>(
            new CommandDefinition(sql, new { TenantId = tenantProvider.GetTenantId() }, cancellationToken: cancellationToken));

        return Result.Success(rows.AsList());
    }
}
