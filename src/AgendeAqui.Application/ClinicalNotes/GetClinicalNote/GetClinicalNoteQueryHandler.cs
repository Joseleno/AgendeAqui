using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.ClinicalNotes;
using AgendeAqui.Domain.Common;
using Dapper;

namespace AgendeAqui.Application.ClinicalNotes.GetClinicalNote;

public sealed class GetClinicalNoteQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider,
    ICurrentUser currentUser) : IQueryHandler<GetClinicalNoteQuery, ClinicalNoteResponse>
{
    public async ValueTask<Result<ClinicalNoteResponse>> Handle(
        GetClinicalNoteQuery query,
        CancellationToken cancellationToken)
    {
        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        var tenantId = tenantProvider.GetTenantId();

        const string sql = """
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
            WHERE cn.id = @Id AND cn.tenant_id = @TenantId
            """;

        var command = new CommandDefinition(
            sql,
            new { query.Id, TenantId = tenantId },
            cancellationToken: cancellationToken);

        var note = await connection.QueryFirstOrDefaultAsync<ClinicalNoteResponse>(command);

        if (note is null)
            return Result.Failure<ClinicalNoteResponse>(ClinicalNoteErrors.NotFound);

        // Visibility: non-admin professionals can only see private notes they authored
        if (note.IsPrivate
            && !currentUser.IsAdmin
            && note.ProfessionalId != currentUser.ProfessionalId)
            return Result.Failure<ClinicalNoteResponse>(ClinicalNoteErrors.NotFound);

        return Result.Success(note);
    }
}
