using AgendeAqui.Api.Auth;
using AgendeAqui.Application.Import.BulkImportClients;
using AgendeAqui.Application.Import.BulkImportProfessionals;
using Mediator;

namespace AgendeAqui.Api.Endpoints;

public static class ImportEndpoints
{
    public static void MapImportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/import")
            .WithTags("Import")
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .RequireRateLimiting("tenant");

        group.MapPost("/professionals", async (BulkImportProfessionalsRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var command = new BulkImportProfessionalsCommand(
                request.Professionals.Select(p => new ImportProfessionalItem(p.Name, p.Email, p.Phone, p.Specialty)).ToList());

            var result = await mediator.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(detail: result.Error.Message, statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code);
        })
        .WithName("BulkImportProfessionals")
        .WithSummary("Bulk import professionals")
        .WithDescription("Imports up to 500 professionals. Existing records (matched by email) are skipped. Returns counts only — no identifying data about skipped records.")
        .Produces<BulkImportResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/clients", async (BulkImportClientsRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var command = new BulkImportClientsCommand(
                request.Clients.Select(c => new ImportClientItem(c.Name, c.Email, c.Phone, c.Notes)).ToList());

            var result = await mediator.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(detail: result.Error.Message, statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code);
        })
        .WithName("BulkImportClients")
        .WithSummary("Bulk import clients")
        .WithDescription("Imports up to 500 clients. Existing records (matched by email) are skipped. Returns counts only.")
        .Produces<BulkImportClientsResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    private sealed record BulkImportProfessionalsRequest(IReadOnlyList<ProfessionalImportItem> Professionals);
    private sealed record ProfessionalImportItem(string Name, string Email, string Phone, string? Specialty);

    private sealed record BulkImportClientsRequest(IReadOnlyList<ClientImportItem> Clients);
    private sealed record ClientImportItem(string Name, string Email, string Phone, string? Notes);
}
