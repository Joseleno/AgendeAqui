using AgendeAqui.Api.Auth;
using AgendeAqui.Application.Tenants.CreateTenant;
using AgendeAqui.Application.Tenants.GetTenant;
using Mediator;

namespace AgendeAqui.Api.Endpoints;

public static class TenantEndpoints
{
    public static void MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/tenants")
            .WithTags("Tenants")
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .RequireRateLimiting("tenant");

        group.MapPost("/", async (CreateTenantRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new CreateTenantCommand(request.Name, request.Slug, request.Plan);
            var result = await mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return Results.Created($"/api/v1/tenants/{result.Value}", new { id = result.Value });

            return Results.Problem(
                detail: result.Error.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: result.Error.Code);
        })
        .WithName("CreateTenant")
        .Produces(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetTenantQuery(id);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status404NotFound,
                    title: result.Error.Code);
        })
        .WithName("GetTenant")
        .Produces<TenantResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}

public sealed record CreateTenantRequest(string Name, string Slug, string Plan);
