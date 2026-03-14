using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.Common;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Appointments;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.Professionals.ListMyPatients;

public sealed class ListMyPatientsQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ICurrentUser currentUser,
    ITenantProvider tenantProvider) : IQueryHandler<ListMyPatientsQuery, PagedResponse<PatientSummaryResponse>>
{
    public async ValueTask<Result<PagedResponse<PatientSummaryResponse>>> Handle(
        ListMyPatientsQuery query,
        CancellationToken cancellationToken)
    {
        if (currentUser.ProfessionalId is null)
            return Result.Failure<PagedResponse<PatientSummaryResponse>>(AppointmentErrors.NotAuthorized);

        var tenantId = tenantProvider.GetTenantId();
        var professionalId = currentUser.ProfessionalId.Value;
        var offset = (query.Page - 1) * query.PageSize;
        var searchPattern = query.Search is not null ? $"%{query.Search}%" : null;

        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        var countSql = """
            SELECT COUNT(DISTINCT c.id)
            FROM clients c
            INNER JOIN appointments a ON a.client_id = c.id AND a.tenant_id = @TenantId
            WHERE a.professional_id = @ProfessionalId
                AND a.tenant_id = @TenantId
                AND (@Search IS NULL OR LOWER(c.name) LIKE LOWER(@SearchPattern))
            """;

        var itemsSql = """
            SELECT
                c.id        AS ClientId,
                c.name      AS Name,
                c.email     AS Email,
                c.phone     AS Phone,
                COUNT(a.id) AS TotalAppointments,
                MAX(a.date) AS LastVisit
            FROM clients c
            INNER JOIN appointments a ON a.client_id = c.id AND a.tenant_id = @TenantId
            WHERE a.professional_id = @ProfessionalId
                AND a.tenant_id = @TenantId
                AND (@Search IS NULL OR LOWER(c.name) LIKE LOWER(@SearchPattern))
            GROUP BY c.id, c.name, c.email, c.phone
            ORDER BY MAX(a.date) DESC
            LIMIT @PageSize OFFSET @Offset
            """;

        var parameters = new
        {
            TenantId = tenantId,
            ProfessionalId = professionalId,
            Search = query.Search,
            SearchPattern = searchPattern,
            query.PageSize,
            Offset = offset
        };

        var countCommand = new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken);
        var totalCount = await connection.ExecuteScalarAsync<int>(countCommand);

        var itemsCommand = new CommandDefinition(itemsSql, parameters, cancellationToken: cancellationToken);
        var items = await connection.QueryAsync<PatientSummaryResponse>(itemsCommand);

        return Result.Success(new PagedResponse<PatientSummaryResponse>(
            items.AsList(),
            query.Page,
            query.PageSize,
            totalCount));
    }
}
