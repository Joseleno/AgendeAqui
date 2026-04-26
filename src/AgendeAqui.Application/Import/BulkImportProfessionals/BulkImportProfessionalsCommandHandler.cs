using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Professionals;
using AgendeAqui.Domain.ValueObjects;
using Dapper;

namespace AgendeAqui.Application.Import.BulkImportProfessionals;

internal sealed class BulkImportProfessionalsCommandHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider) : ICommandHandler<BulkImportProfessionalsCommand, BulkImportResult>
{
    public async ValueTask<Result<BulkImportResult>> Handle(
        BulkImportProfessionalsCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.GetTenantId();
        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        // Use INSERT ... ON CONFLICT DO NOTHING to avoid TOCTOU race on concurrent imports.
        // Returns count of rows actually inserted (skipped rows return nothing).
        const string upsertSql = """
            INSERT INTO professionals (id, tenant_id, name, email, phone, specialty, is_active, created_at)
            VALUES (@Id, @TenantId, @Name, @Email, @Phone, @Specialty, true, @CreatedAt)
            ON CONFLICT (tenant_id, email) DO NOTHING
            """;

        var now = DateTime.UtcNow;
        var imported = 0;
        var skipped = 0;

        foreach (var item in command.Professionals)
        {
            var emailResult = Email.Create(item.Email);
            if (emailResult.IsFailure) { skipped++; continue; }

            var phoneResult = PhoneNumber.Create(item.Phone);
            if (phoneResult.IsFailure) { skipped++; continue; }

            var specialty = item.Specialty is not null
                ? Specialty.Create(item.Specialty)
                : null;

            if (specialty?.IsFailure == true) { skipped++; continue; }

            var rows = await connection.ExecuteAsync(
                new CommandDefinition(upsertSql, new
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    Name = item.Name.Trim(),
                    Email = emailResult.Value.Value,
                    Phone = phoneResult.Value.Value,
                    Specialty = specialty?.Value.Value,
                    CreatedAt = now
                }, cancellationToken: cancellationToken));

            if (rows > 0) imported++;
            else skipped++;
        }

        return Result.Success(new BulkImportResult(imported, skipped));
    }
}
