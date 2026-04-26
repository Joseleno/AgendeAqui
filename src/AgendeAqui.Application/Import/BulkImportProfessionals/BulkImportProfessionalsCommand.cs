using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Import.BulkImportProfessionals;

public sealed record BulkImportProfessionalsCommand(
    IReadOnlyList<ImportProfessionalItem> Professionals) : ICommand<BulkImportResult>;

public sealed record ImportProfessionalItem(
    string Name,
    string Email,
    string Phone,
    string? Specialty);

public sealed record BulkImportResult(int Imported, int Skipped);
