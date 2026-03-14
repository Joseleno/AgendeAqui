using AgendeAqui.Api.Auth;
using AgendeAqui.Api.Endpoints.Requests;
using AgendeAqui.Application.Professionals.CreateProfessional;
using AgendeAqui.Application.Professionals.GetProfessional;
using AgendeAqui.Application.Professionals.LinkService;
using AgendeAqui.Application.Professionals.ListProfessionals;
using AgendeAqui.Application.Professionals.UnlinkService;
using AgendeAqui.Application.Professionals.UpdateProfessional;
using AgendeAqui.Domain.Abstractions;
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
            var command = new CreateProfessionalCommand(request.Name, request.Email, request.Phone, request.Specialty);
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

        group.MapGet("/", async (int? page, int? pageSize, string? specialty, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new ListProfessionalsQuery(page ?? 1, pageSize ?? 10, specialty);
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
            var command = new UpdateProfessionalCommand(id, request.Name, request.Email, request.Phone, request.Specialty);
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

        // Professional-Service link management
        group.MapPost("/{id:guid}/services", async (Guid id, LinkServiceRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new LinkServiceToProfessionalCommand(id, request.ServiceId);
            var result = await mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return Results.NoContent();

            var statusCode = result.Error.IsNotFound
                ? StatusCodes.Status404NotFound
                : result.Error.Code.EndsWith(".ServiceAlreadyLinked", StringComparison.Ordinal)
                    ? StatusCodes.Status409Conflict
                    : StatusCodes.Status400BadRequest;

            return Results.Problem(
                detail: result.Error.Message,
                statusCode: statusCode,
                title: result.Error.Code);
        })
        .WithName("LinkServiceToProfessional")
        .WithSummary("Link service to professional")
        .WithDescription("Associates a service with a professional.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .RequireAuthorization(AuthorizationPolicies.RequireAdmin);

        group.MapDelete("/{id:guid}/services/{serviceId:guid}", async (Guid id, Guid serviceId, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new UnlinkServiceFromProfessionalCommand(id, serviceId);
            var result = await mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return Results.NoContent();

            return Results.Problem(
                detail: result.Error.Message,
                statusCode: result.Error.IsNotFound ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest,
                title: result.Error.Code);
        })
        .WithName("UnlinkServiceFromProfessional")
        .WithSummary("Unlink service from professional")
        .WithDescription("Removes the association between a service and a professional.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(AuthorizationPolicies.RequireAdmin);

        group.MapGet("/{id:guid}/services", async (Guid id, IProfessionalServiceRepository repository, CancellationToken cancellationToken) =>
        {
            var serviceIds = await repository.GetServiceIdsByProfessionalAsync(id, cancellationToken);
            return Results.Ok(serviceIds);
        })
        .WithName("GetProfessionalServices")
        .WithSummary("Get services linked to a professional")
        .WithDescription("Returns the list of service IDs linked to a professional.")
        .Produces<IReadOnlyList<Guid>>()
        .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated);
    }
}
