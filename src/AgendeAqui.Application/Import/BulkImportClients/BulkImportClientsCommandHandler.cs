using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.ValueObjects;
using Dapper;

namespace AgendeAqui.Application.Import.BulkImportClients;

internal sealed class BulkImportClientsCommandHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    ITenantProvider tenantProvider) : ICommandHandler<BulkImportClientsCommand, BulkImportClientsResult>
{
    public async ValueTask<Result<BulkImportClientsResult>> Handle(
        BulkImportClientsCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.GetTenantId();
        using var connection = await sqlConnectionFactory.CreateConnectionAsync(cancellationToken);

        const string upsertSql = """
            INSERT INTO clients (id, tenant_id, name, email, phone, notes, created_at)
            VALUES (@Id, @TenantId, @Name, @Email, @Phone, @Notes, @CreatedAt)
            ON CONFLICT (tenant_id, email) DO NOTHING
            """;

        var now = DateTime.UtcNow;
        var imported = 0;
        var skipped = 0;

        foreach (var item in command.Clients)
        {
            var emailResult = Email.Create(item.Email);
            if (emailResult.IsFailure) { skipped++; continue; }

            var phoneResult = PhoneNumber.Create(item.Phone);
            if (phoneResult.IsFailure) { skipped++; continue; }

            // Sanitize notes: truncate silently (validator already enforces max length)
            var notes = item.Notes?.Trim();

            var rows = await connection.ExecuteAsync(
                new CommandDefinition(upsertSql, new
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    Name = item.Name.Trim(),
                    Email = emailResult.Value.Value,
                    Phone = phoneResult.Value.Value,
                    Notes = notes,
                    CreatedAt = now
                }, cancellationToken: cancellationToken));

            if (rows > 0) imported++;
            else skipped++;
        }

        return Result.Success(new BulkImportClientsResult(imported, skipped));
    }
}
