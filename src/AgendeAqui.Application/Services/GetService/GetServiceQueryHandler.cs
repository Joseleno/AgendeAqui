using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Services;
using Dapper;

namespace AgendeAqui.Application.Services.GetService;

public sealed class GetServiceQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory) : IQueryHandler<GetServiceQuery, ServiceResponse>
{
    public async ValueTask<Result<ServiceResponse>> Handle(
        GetServiceQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT id AS Id,
                   name AS Name,
                   EXTRACT(EPOCH FROM duration) / 60 AS DurationMinutes,
                   price AS Price,
                   is_active AS IsActive,
                   created_at AS CreatedAt
            FROM services
            WHERE id = @ServiceId
            """;

        var result = await connection.QueryFirstOrDefaultAsync<ServiceResponse>(
            sql,
            new { query.ServiceId });

        if (result is null)
            return Result.Failure<ServiceResponse>(ServiceErrors.NotFound);

        return Result.Success(result);
    }
}
