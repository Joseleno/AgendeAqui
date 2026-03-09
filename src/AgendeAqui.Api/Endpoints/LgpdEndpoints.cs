using AgendeAqui.Api.Auth;
using AgendeAqui.Application.Clients.DeleteClientData;
using AgendeAqui.Application.Clients.ExportClientData;
using Mediator;

namespace AgendeAqui.Api.Endpoints;

public static class LgpdEndpoints
{
    public static void MapLgpdEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/clients/{clientId:guid}/data")
            .WithTags("LGPD")
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .RequireRateLimiting("tenant");

        group.MapDelete("/", async (Guid clientId, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new DeleteClientDataCommand(clientId);
            var result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: result.Error.IsNotFound
                        ? StatusCodes.Status404NotFound
                        : StatusCodes.Status400BadRequest,
                    title: result.Error.Code);
        })
        .WithName("DeleteClientData")
        .WithSummary("Delete client data (LGPD)")
        .WithDescription("Anonymizes client data in compliance with LGPD data protection law.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/export", async (Guid clientId, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new ExportClientDataQuery(clientId);
            var result = await mediator.Send(query, cancellationToken);

            if (result.IsSuccess)
                return Results.Ok(result.Value);

            return Results.Problem(
                detail: result.Error.Message,
                statusCode: result.Error.IsNotFound
                    ? StatusCodes.Status404NotFound
                    : StatusCodes.Status400BadRequest,
                title: result.Error.Code);
        })
        .WithName("ExportClientData")
        .WithSummary("Export client data (LGPD)")
        .WithDescription("Exports all client data including appointment history for LGPD compliance.")
        .Produces<ClientDataExport>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
