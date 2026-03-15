using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Application.ClinicalNotes.GetClinicalNote;
using AgendeAqui.Application.Common;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.ClinicalNotes.ListClinicalNotes;

public sealed class ListClinicalNotesQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider,
    ICurrentUser currentUser) : IQueryHandler<ListClinicalNotesQuery, PagedResponse<ClinicalNoteResponse>>
{
    private const string CountAllSql =
        "SELECT COUNT(*) FROM clinical_notes cn WHERE cn.tenant_id = @TenantId AND cn.client_id = @ClientId";

    private const string CountVisibleSql =
        "SELECT COUNT(*) FROM clinical_notes cn WHERE cn.tenant_id = @TenantId AND cn.client_id = @ClientId AND (cn.is_private = false OR cn.professional_id = @CurrentProfessionalId)";

    private const string ItemsAllSql = """
        SELECT cn.id               AS Id,
               cn.professional_id   AS ProfessionalId,
               p.name              AS ProfessionalName,
               cn.client_id        AS ClientId,
               cn.appointment_id   AS AppointmentId,
               cn.title            AS Title,
               cn.content          AS Content,
               cn.is_private       AS IsPrivate,
               cn.created_at       AS CreatedAt,
               cn.updated_at       AS UpdatedAt
        FROM clinical_notes cn
        INNER JOIN professionals p ON p.id = cn.professional_id AND p.tenant_id = @TenantId
        WHERE cn.tenant_id = @TenantId AND cn.client_id = @ClientId
        ORDER BY cn.created_at DESC
        LIMIT @PageSize OFFSET @Offset
        """;

    private const string ItemsVisibleSql = """
        SELECT cn.id               AS Id,
               cn.professional_id   AS ProfessionalId,
               p.name              AS ProfessionalName,
               cn.client_id        AS ClientId,
               cn.appointment_id   AS AppointmentId,
               cn.title            AS Title,
               cn.content          AS Content,
               cn.is_private       AS IsPrivate,
               cn.created_at       AS CreatedAt,
               cn.updated_at       AS UpdatedAt
        FROM clinical_notes cn
        INNER JOIN professionals p ON p.id = cn.professional_id AND p.tenant_id = @TenantId
        WHERE cn.tenant_id = @TenantId AND cn.client_id = @ClientId
            AND (cn.is_private = false OR cn.professional_id = @CurrentProfessionalId)
        ORDER BY cn.created_at DESC
        LIMIT @PageSize OFFSET @Offset
        """;

    public async ValueTask<Result<PagedResponse<ClinicalNoteResponse>>> Handle(
        ListClinicalNotesQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        var tenantId = tenantProvider.GetTenantId();
        var parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantId);
        parameters.Add("ClientId", query.ClientId);

        // Visibility rules:
        // Admin sees everything.
        // Professional sees all non-private notes + their own private notes.
        var needsVisibilityFilter = !currentUser.IsAdmin && currentUser.ProfessionalId.HasValue;
        if (needsVisibilityFilter)
            parameters.Add("CurrentProfessionalId", currentUser.ProfessionalId!.Value);

        var countSql = needsVisibilityFilter ? CountVisibleSql : CountAllSql;
        var countCommand = new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken);
        var totalCount = await connection.ExecuteScalarAsync<int>(countCommand);

        var offset = (query.Page - 1) * query.PageSize;
        parameters.Add("PageSize", query.PageSize);
        parameters.Add("Offset", offset);

        var itemsSql = needsVisibilityFilter ? ItemsVisibleSql : ItemsAllSql;
        var itemsCommand = new CommandDefinition(itemsSql, parameters, cancellationToken: cancellationToken);
        var items = await connection.QueryAsync<ClinicalNoteResponse>(itemsCommand);

        var response = new PagedResponse<ClinicalNoteResponse>(
            items.AsList(),
            query.Page,
            query.PageSize,
            totalCount);

        return Result.Success(response);
    }
}
