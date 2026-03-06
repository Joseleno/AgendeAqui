using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Professionals;
using Dapper;

namespace AgendeAqui.Application.Professionals.GetProfessional;

public sealed class GetProfessionalQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory) : IQueryHandler<GetProfessionalQuery, ProfessionalResponse>
{
    public async ValueTask<Result<ProfessionalResponse>> Handle(
        GetProfessionalQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT id          AS Id,
                   name        AS Name,
                   email       AS Email,
                   phone       AS Phone,
                   is_active   AS IsActive,
                   created_at  AS CreatedAt
            FROM professionals
            WHERE id = @ProfessionalId
            """;

        var professional = await connection.QueryFirstOrDefaultAsync<ProfessionalResponse>(
            sql,
            new { query.ProfessionalId });

        if (professional is null)
            return Result.Failure<ProfessionalResponse>(ProfessionalErrors.NotFound);

        return Result.Success(professional);
    }
}
