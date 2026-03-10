using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Services;
using Dapper;

namespace AgendeAqui.Application.Services.GetService;

public sealed class GetServiceQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider) : IQueryHandler<GetServiceQuery, ServiceResponse>
{
    public async ValueTask<Result<ServiceResponse>> Handle(
        GetServiceQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        const string sql = """
            SELECT id AS Id,
                   name AS Name,
                   CAST(EXTRACT(EPOCH FROM duration) / 60 AS integer) AS DurationMinutes,
                   price AS Price,
                   is_active AS IsActive,
                   created_at AS CreatedAt
            FROM services
            WHERE id = @ServiceId AND tenant_id = @TenantId
            """;

        var command = new CommandDefinition(
            sql,
            new { query.ServiceId, TenantId = tenantProvider.GetTenantId() },
            cancellationToken: cancellationToken);

        var result = await connection.QueryFirstOrDefaultAsync<ServiceResponse>(command);

        if (result is null)
            return Result.Failure<ServiceResponse>(ServiceErrors.NotFound);

        return Result.Success(result);
    }
}
