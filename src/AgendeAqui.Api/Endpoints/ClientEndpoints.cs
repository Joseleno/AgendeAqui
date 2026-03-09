using AgendeAqui.Api.Auth;
using AgendeAqui.Api.Endpoints.Requests;
using AgendeAqui.Application.Clients.CreateClient;
using AgendeAqui.Application.Clients.GetClient;
using AgendeAqui.Application.Clients.ListClients;
using AgendeAqui.Application.Clients.UpdateClient;
using Mediator;

namespace AgendeAqui.Api.Endpoints;

public static class ClientEndpoints
{
    public static void MapClientEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/clients")
            .WithTags("Clients")
            .RequireRateLimiting("tenant");

        group.MapPost("/", async (CreateClientRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new CreateClientCommand(request.Name, request.Email, request.Phone);
            var result = await mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return Results.Created($"/api/v1/clients/{result.Value}", new { id = result.Value });

            return Results.Problem(
                detail: result.Error.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: result.Error.Code);
        })
        .WithName("CreateClient")
        .WithSummary("Create client")
        .WithDescription("Registers a new client for the current tenant.")
        .Produces(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .RequireAuthorization(AuthorizationPolicies.RequireProfessional);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetClientQuery(id);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status404NotFound,
                    title: result.Error.Code);
        })
        .WithName("GetClient")
        .WithSummary("Get client by ID")
        .WithDescription("Retrieves client details.")
        .Produces<ClientResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated);

        group.MapGet("/", async (int? page, int? pageSize, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new ListClientsQuery(page ?? 1, pageSize ?? 10);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: result.Error.Code);
        })
        .WithName("ListClients")
        .WithSummary("List clients")
        .WithDescription("Lists clients for the current tenant with optional search. Supports pagination.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated);

        group.MapPut("/{id:guid}", async (Guid id, UpdateClientRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new UpdateClientCommand(id, request.Name, request.Email, request.Phone);
            var result = await mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return Results.NoContent();

            return result.Error.IsNotFound
                ? Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status404NotFound,
                    title: result.Error.Code)
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: result.Error.Code);
        })
        .WithName("UpdateClient")
        .WithSummary("Update client")
        .WithDescription("Updates client details.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(AuthorizationPolicies.RequireProfessional);
    }
}
