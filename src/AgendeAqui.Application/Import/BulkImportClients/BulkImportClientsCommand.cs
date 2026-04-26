using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Import.BulkImportClients;

public sealed record BulkImportClientsCommand(
    IReadOnlyList<ImportClientItem> Clients) : ICommand<BulkImportClientsResult>;

public sealed record ImportClientItem(
    string Name,
    string Email,
    string Phone,
    string? Notes);

public sealed record BulkImportClientsResult(int Imported, int Skipped);
