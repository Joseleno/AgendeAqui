using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Common;
using AgendeAqui.Application.Professionals.GetProfessional;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.Professionals.ListProfessionals;

public sealed class ListProfessionalsQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory) : IQueryHandler<ListProfessionalsQuery, PagedResponse<ProfessionalResponse>>
{
    public async ValueTask<Result<PagedResponse<ProfessionalResponse>>> Handle(
        ListProfessionalsQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = sqlConnectionFactory.CreateConnection();

        const string countSql = """
            SELECT COUNT(*)
            FROM professionals
            """;

        const string itemsSql = """
            SELECT id          AS Id,
                   name        AS Name,
                   email       AS Email,
                   phone       AS Phone,
                   is_active   AS IsActive,
                   created_at  AS CreatedAt
            FROM professionals
            ORDER BY name
            LIMIT @PageSize OFFSET @Offset
            """;

        var totalCount = await connection.ExecuteScalarAsync<int>(countSql);

        var offset = (query.Page - 1) * query.PageSize;

        var items = await connection.QueryAsync<ProfessionalResponse>(
            itemsSql,
            new { query.PageSize, Offset = offset });

        var response = new PagedResponse<ProfessionalResponse>(
            items.AsList(),
            query.Page,
            query.PageSize,
            totalCount);

        return Result.Success(response);
    }
}
