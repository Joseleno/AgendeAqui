using AgendeAqui.Api.Endpoints.Requests;
using AgendeAqui.Application.Services.CreateService;
using AgendeAqui.Application.Services.GetService;
using AgendeAqui.Application.Services.ListServices;
using AgendeAqui.Application.Services.UpdateService;
using Mediator;

namespace AgendeAqui.Api.Endpoints;

public static class ServiceEndpoints
{
    public static void MapServiceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/services")
            .WithTags("Services");

        group.MapPost("/", async (CreateServiceRequest request, IMediator mediator) =>
        {
            var command = new CreateServiceCommand(request.Name, request.DurationMinutes, request.Price);
            var result = await mediator.Send(command);

            return result.IsSuccess
                ? Results.Created($"/api/v1/services/{result.Value}", new { id = result.Value })
                : Results.BadRequest(new { error = result.Error.Message });
        })
        .WithName("CreateService")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var query = new GetServiceQuery(id);
            var result = await mediator.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { error = result.Error.Message });
        })
        .WithName("GetService")
        .Produces<ServiceResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", async (int? page, int? pageSize, IMediator mediator) =>
        {
            var query = new ListServicesQuery(page ?? 1, pageSize ?? 10);
            var result = await mediator.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error.Message });
        })
        .WithName("ListServices")
        .Produces(StatusCodes.Status200OK);

        group.MapPut("/{id:guid}", async (Guid id, UpdateServiceRequest request, IMediator mediator) =>
        {
            var command = new UpdateServiceCommand(id, request.Name, request.DurationMinutes, request.Price);
            var result = await mediator.Send(command);

            if (result.IsSuccess)
                return Results.NoContent();

            return result.Error.IsNotFound
                ? Results.NotFound(new { error = result.Error.Message })
                : Results.BadRequest(new { error = result.Error.Message });
        })
        .WithName("UpdateService")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}
