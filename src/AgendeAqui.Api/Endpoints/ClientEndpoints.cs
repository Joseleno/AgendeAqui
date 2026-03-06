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
            .WithTags("Clients");

        group.MapPost("/", async (CreateClientRequest request, IMediator mediator) =>
        {
            var command = new CreateClientCommand(request.Name, request.Email, request.Phone);
            var result = await mediator.Send(command);

            return result.IsSuccess
                ? Results.Created($"/api/v1/clients/{result.Value}", new { id = result.Value })
                : Results.BadRequest(new { error = result.Error.Message });
        })
        .WithName("CreateClient")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var query = new GetClientQuery(id);
            var result = await mediator.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { error = result.Error.Message });
        })
        .WithName("GetClient")
        .Produces<ClientResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", async (int? page, int? pageSize, IMediator mediator) =>
        {
            var query = new ListClientsQuery(page ?? 1, pageSize ?? 10);
            var result = await mediator.Send(query);

            return Results.Ok(result.Value);
        })
        .WithName("ListClients")
        .Produces(StatusCodes.Status200OK);

        group.MapPut("/{id:guid}", async (Guid id, UpdateClientRequest request, IMediator mediator) =>
        {
            var command = new UpdateClientCommand(id, request.Name, request.Email, request.Phone);
            var result = await mediator.Send(command);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(new { error = result.Error.Message });
        })
        .WithName("UpdateClient")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

public sealed record CreateClientRequest(string Name, string Email, string Phone);
public sealed record UpdateClientRequest(string Name, string Email, string Phone);
