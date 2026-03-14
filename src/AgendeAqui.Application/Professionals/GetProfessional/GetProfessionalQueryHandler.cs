using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Professionals;
using Dapper;

namespace AgendeAqui.Application.Professionals.GetProfessional;

public sealed class GetProfessionalQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider) : IQueryHandler<GetProfessionalQuery, ProfessionalResponse>
{
    public async ValueTask<Result<ProfessionalResponse>> Handle(
        GetProfessionalQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        const string sql = """
            SELECT id          AS Id,
                   name        AS Name,
                   email       AS Email,
                   phone       AS Phone,
                   is_active   AS IsActive,
                   specialty   AS Specialty,
                   created_at  AS CreatedAt
            FROM professionals
            WHERE id = @ProfessionalId AND tenant_id = @TenantId
            """;

        var command = new CommandDefinition(
            sql,
            new { query.ProfessionalId, TenantId = tenantProvider.GetTenantId() },
            cancellationToken: cancellationToken);

        var professional = await connection.QueryFirstOrDefaultAsync<ProfessionalResponse>(command);

        if (professional is null)
            return Result.Failure<ProfessionalResponse>(ProfessionalErrors.NotFound);

        return Result.Success(professional);
    }
}
