using AgendeAqui.Api.Auth;
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
            .WithTags("Services")
            .RequireRateLimiting("tenant");

        group.MapPost("/", async (CreateServiceRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new CreateServiceCommand(request.Name, request.DurationMinutes, request.Price);
            var result = await mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return Results.Created($"/api/v1/services/{result.Value}", new { id = result.Value });

            return Results.Problem(
                detail: result.Error.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: result.Error.Code);
        })
        .WithName("CreateService")
        .WithSummary("Create service")
        .WithDescription("Creates a new service with name, duration, and price.")
        .Produces(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .RequireAuthorization(AuthorizationPolicies.RequireAdmin);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetServiceQuery(id);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status404NotFound,
                    title: result.Error.Code);
        })
        .WithName("GetService")
        .WithSummary("Get service by ID")
        .WithDescription("Retrieves service details.")
        .Produces<ServiceResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated);

        group.MapGet("/", async (int? page, int? pageSize, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new ListServicesQuery(page ?? 1, pageSize ?? 10);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: result.Error.Code);
        })
        .WithName("ListServices")
        .WithSummary("List services")
        .WithDescription("Lists active services for the current tenant. Supports pagination.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated);

        group.MapPut("/{id:guid}", async (Guid id, UpdateServiceRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new UpdateServiceCommand(id, request.Name, request.DurationMinutes, request.Price);
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
        .WithName("UpdateService")
        .WithSummary("Update service")
        .WithDescription("Updates service details.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(AuthorizationPolicies.RequireAdmin);
    }
}
