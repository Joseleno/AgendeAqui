using AgendeAqui.Api.Auth;
using AgendeAqui.Api.Endpoints.Requests;
using AgendeAqui.Application.Professionals.CreateProfessional;
using AgendeAqui.Application.Professionals.GetProfessional;
using AgendeAqui.Application.Professionals.ListProfessionals;
using AgendeAqui.Application.Professionals.UpdateProfessional;
using Mediator;

namespace AgendeAqui.Api.Endpoints;

public static class ProfessionalEndpoints
{
    public static void MapProfessionalEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/professionals")
            .WithTags("Professionals")
            .RequireRateLimiting("tenant");

        group.MapPost("/", async (CreateProfessionalRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new CreateProfessionalCommand(request.Name, request.Email, request.Phone);
            var result = await mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return Results.Created($"/api/v1/professionals/{result.Value}", new { id = result.Value });

            return Results.Problem(
                detail: result.Error.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: result.Error.Code);
        })
        .WithName("CreateProfessional")
        .WithSummary("Create professional")
        .WithDescription("Registers a new professional for the current tenant.")
        .Produces(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .RequireAuthorization(AuthorizationPolicies.RequireAdmin);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetProfessionalQuery(id);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status404NotFound,
                    title: result.Error.Code);
        })
        .WithName("GetProfessional")
        .WithSummary("Get professional by ID")
        .WithDescription("Retrieves professional details.")
        .Produces<ProfessionalResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated);

        group.MapGet("/", async (int? page, int? pageSize, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new ListProfessionalsQuery(page ?? 1, pageSize ?? 10);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: result.Error.Code);
        })
        .WithName("ListProfessionals")
        .WithSummary("List professionals")
        .WithDescription("Lists active professionals for the current tenant. Supports pagination.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated);

        group.MapPut("/{id:guid}", async (Guid id, UpdateProfessionalRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new UpdateProfessionalCommand(id, request.Name, request.Email, request.Phone);
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
        .WithName("UpdateProfessional")
        .WithSummary("Update professional")
        .WithDescription("Updates professional details including name, email, and phone.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(AuthorizationPolicies.RequireAdmin);
    }
}
