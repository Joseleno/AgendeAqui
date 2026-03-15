using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.Payments.GetPaymentSummary;

public sealed class GetPaymentSummaryQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider) : IQueryHandler<GetPaymentSummaryQuery, PaymentSummaryResponse>
{
    public async ValueTask<Result<PaymentSummaryResponse>> Handle(
        GetPaymentSummaryQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        var tenantId = tenantProvider.GetTenantId();

        const string summarySql = """
            SELECT COALESCE(SUM(amount) FILTER (WHERE status = 'Paid'), 0)     AS TotalReceived,
                   COALESCE(SUM(amount) FILTER (WHERE status = 'Pending'), 0)  AS TotalPending,
                   COALESCE(SUM(amount) FILTER (WHERE status = 'Refunded'), 0) AS TotalRefunded,
                   CAST(COUNT(*) AS integer) AS PaymentCount
            FROM payments
            WHERE tenant_id = @TenantId
                AND created_at >= @From AND created_at < @To
            """;

        const string byMethodSql = """
            SELECT method  AS Method,
                   COALESCE(SUM(amount) FILTER (WHERE status = 'Paid'), 0) AS Total,
                   CAST(COUNT(*) AS integer) AS Count
            FROM payments
            WHERE tenant_id = @TenantId
                AND created_at >= @From AND created_at < @To
            GROUP BY method
            ORDER BY Total DESC
            """;

        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);
        parameters.Add("From", query.From.ToDateTime(TimeOnly.MinValue), System.Data.DbType.DateTime);
        parameters.Add("To", query.To.AddDays(1).ToDateTime(TimeOnly.MinValue), System.Data.DbType.DateTime);

        var summary = await connection.QuerySingleAsync<SummaryRow>(
            new CommandDefinition(summarySql, parameters, cancellationToken: cancellationToken));

        var byMethod = await connection.QueryAsync<MethodSummary>(
            new CommandDefinition(byMethodSql, parameters, cancellationToken: cancellationToken));

        var response = new PaymentSummaryResponse(
            summary.TotalReceived,
            summary.TotalPending,
            summary.TotalRefunded,
            summary.PaymentCount,
            byMethod.AsList());

        return Result.Success(response);
    }

    private sealed record SummaryRow(
        decimal TotalReceived,
        decimal TotalPending,
        decimal TotalRefunded,
        int PaymentCount);
}
