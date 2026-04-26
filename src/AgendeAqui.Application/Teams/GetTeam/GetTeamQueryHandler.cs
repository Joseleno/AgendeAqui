using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Teams;
using Dapper;

namespace AgendeAqui.Application.Teams.GetTeam;

internal sealed class GetTeamQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider) : IQueryHandler<GetTeamQuery, TeamDetailResponse>
{
    public async ValueTask<Result<TeamDetailResponse>> Handle(GetTeamQuery query, CancellationToken cancellationToken)
    {
        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        const string teamSql = """
            SELECT t.id, t.name, t.description, t.leader_id AS LeaderId,
                   p.name AS LeaderName, t.is_active AS IsActive, t.created_at AS CreatedAt
            FROM teams t
            LEFT JOIN professionals p ON p.id = t.leader_id AND p.tenant_id = @TenantId
            WHERE t.id = @TeamId AND t.tenant_id = @TenantId
            """;

        const string memberSql = """
            SELECT tm.professional_id AS ProfessionalId,
                   p.name             AS ProfessionalName,
                   p.specialty        AS Specialty,
                   tm.joined_at       AS JoinedAt
            FROM team_members tm
            INNER JOIN professionals p ON p.id = tm.professional_id AND p.tenant_id = @TenantId
            WHERE tm.team_id = @TeamId AND tm.tenant_id = @TenantId AND tm.left_at IS NULL
            ORDER BY p.name
            """;

        var parameters = new { TenantId = tenantProvider.GetTenantId(), TeamId = query.TeamId };

        var teamRow = await connection.QuerySingleOrDefaultAsync<dynamic>(
            new CommandDefinition(teamSql, parameters, cancellationToken: cancellationToken));

        if (teamRow is null)
            return Result.Failure<TeamDetailResponse>(TeamErrors.NotFound);

        var members = await connection.QueryAsync<TeamMemberResponse>(
            new CommandDefinition(memberSql, parameters, cancellationToken: cancellationToken));

        var response = new TeamDetailResponse(
            Id: (Guid)teamRow.id,
            Name: (string)teamRow.name,
            Description: (string?)teamRow.description,
            LeaderId: (Guid?)teamRow.leaderid,
            LeaderName: (string?)teamRow.leadername,
            IsActive: (bool)teamRow.isactive,
            CreatedAt: (DateTime)teamRow.createdat,
            Members: members.AsList());

        return Result.Success(response);
    }
}
