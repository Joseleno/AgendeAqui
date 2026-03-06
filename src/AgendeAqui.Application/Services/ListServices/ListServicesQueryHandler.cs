using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Common;
using AgendeAqui.Application.Services.GetService;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.Services.ListServices;

public sealed class ListServicesQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory) : IQueryHandler<ListServicesQuery, PagedResponse<ServiceResponse>>
{
    public async ValueTask<Result<PagedResponse<ServiceResponse>>> Handle(
        ListServicesQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = sqlConnectionFactory.CreateConnection();

        const string countSql = """
            SELECT COUNT(*)
            FROM services
            WHERE is_active = true
            """;

        const string itemsSql = """
            SELECT id AS Id,
                   name AS Name,
                   EXTRACT(EPOCH FROM duration) / 60 AS DurationMinutes,
                   price AS Price,
                   is_active AS IsActive,
                   created_at AS CreatedAt
            FROM services
            WHERE is_active = true
            ORDER BY name
            LIMIT @PageSize OFFSET @Offset
            """;

        var totalCount = await connection.ExecuteScalarAsync<int>(countSql);

        var offset = (query.Page - 1) * query.PageSize;

        var items = await connection.QueryAsync<ServiceResponse>(
            itemsSql,
            new { query.PageSize, Offset = offset });

        var response = new PagedResponse<ServiceResponse>(
            items.ToList().AsReadOnly(),
            query.Page,
            query.PageSize,
            totalCount);

        return Result.Success(response);
    }
}
