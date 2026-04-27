using AgendeAqui.Api.Auth;
using AgendeAqui.Application.Common;
using AgendeAqui.Application.Tenants.CreateTenant;
using AgendeAqui.Application.Tenants.GetTenant;
using AgendeAqui.Application.Tenants.GetTenantContext;
using AgendeAqui.Application.Tenants.ListTenants;
using AgendeAqui.Application.Tenants.UpdateTenant;
using AgendeAqui.Application.Tenants.UpdateTenantTheme;
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
        .WithSummary("Create a new tenant")
        .WithDescription("Creates a new tenant with the specified name, slug, and plan.")
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
        .WithSummary("Get tenant by ID")
        .WithDescription("Retrieves tenant details including name, slug, status, and plan.")
        .Produces<TenantResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/", async (int? page, int? pageSize, string? status, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new ListTenantsQuery
            {
                Page = page ?? 1,
                PageSize = pageSize ?? 10,
                Status = status
            };
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: result.Error.Code);
        })
        .WithName("ListTenants")
        .WithSummary("List tenants")
        .WithDescription("Lists all tenants with optional status filter. Supports pagination. Admin only.")
        .Produces<PagedResponse<TenantResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", async (Guid id, UpdateTenantRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new UpdateTenantCommand(id, request.Name, request.Plan);
            var result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: result.Error.Code.Contains("NotFound") ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest,
                    title: result.Error.Code);
        })
        .WithName("UpdateTenant")
        .WithSummary("Update tenant")
        .WithDescription("Updates the tenant name and plan.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        app.MapGet("/api/v1/tenant/context", async (IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetTenantContextQuery(), cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(detail: result.Error.Message, statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code);
        })
        .WithTags("Tenants")
        .WithName("GetTenantContext")
        .WithSummary("Get tenant context")
        .WithDescription("Returns the current tenant's labels, features, theme, plan, and trial info.")
        .Produces<TenantContextResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated)
        .RequireRateLimiting("tenant");

        // Theme management — admin only within a tenant
        app.MapPut("/api/v1/tenant/theme", async (UpdateTenantThemeRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new UpdateTenantThemeCommand(request.PrimaryColor, request.LogoUrl, request.FaviconUrl);
            var result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(detail: result.Error.Message, statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code);
        })
        .WithTags("Tenants")
        .WithName("UpdateTenantTheme")
        .WithSummary("Update tenant theme")
        .WithDescription("Updates the visual theme (primary color, logo, favicon) for the current tenant.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
        .RequireRateLimiting("tenant");
    }
}

public sealed record CreateTenantRequest(string Name, string Slug, string Plan);

public sealed record UpdateTenantRequest(string Name, string Plan);

public sealed record UpdateTenantThemeRequest(string PrimaryColor, string? LogoUrl, string? FaviconUrl);
