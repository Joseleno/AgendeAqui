using AgendeAqui.Api.Auth;
using AgendeAqui.Application.Reports.GetPlatformDashboard;
using AgendeAqui.Application.Tenants.ActivateTenant;
using AgendeAqui.Application.Tenants.ChangeTenantPlan;
using AgendeAqui.Application.Tenants.GetTenantUsage;
using AgendeAqui.Application.Tenants.SetCustomDomain;
using AgendeAqui.Application.Tenants.SuspendTenant;
using Mediator;

namespace AgendeAqui.Api.Endpoints;

public static class PlatformEndpoints
{
    public static void MapPlatformEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/platform")
            .WithTags("Platform")
            .RequireAuthorization(AuthorizationPolicies.RequirePlatformOperator)
            .RequireRateLimiting("tenant");

        group.MapGet("/dashboard", async (DateOnly from, DateOnly to, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetPlatformDashboardQuery(from, to);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(detail: result.Error.Message, statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code);
        })
        .WithName("GetPlatformDashboard")
        .WithSummary("Platform dashboard")
        .WithDescription("Cross-tenant overview: active tenants, total appointments, per-tenant breakdown. Requires platform_role claim.")
        .Produces<PlatformDashboardResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/tenants/{id:guid}/usage", async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetTenantUsageQuery(id), cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(detail: result.Error.Message,
                    statusCode: result.Error.Code.Contains("NotFound") ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest,
                    title: result.Error.Code);
        })
        .WithName("GetTenantUsage")
        .WithSummary("Get tenant usage")
        .WithDescription("Returns usage metrics for a tenant relative to their plan limits.")
        .Produces<TenantUsageResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/tenants/{id:guid}/suspend", async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new SuspendTenantCommand(id), cancellationToken);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(detail: result.Error.Message,
                    statusCode: result.Error.Code.Contains("NotFound") ? StatusCodes.Status404NotFound : StatusCodes.Status409Conflict,
                    title: result.Error.Code);
        })
        .WithName("SuspendTenant")
        .WithSummary("Suspend tenant")
        .WithDescription("Suspends a tenant, blocking all access. Operator action.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/tenants/{id:guid}/activate", async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new ActivateTenantCommand(id), cancellationToken);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(detail: result.Error.Message,
                    statusCode: result.Error.Code.Contains("NotFound") ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest,
                    title: result.Error.Code);
        })
        .WithName("ActivateTenant")
        .WithSummary("Activate tenant")
        .WithDescription("Re-activates a suspended or inactive tenant. Operator action.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/tenants/{id:guid}/plan", async (Guid id, ChangePlanRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new ChangeTenantPlanCommand(id, request.Plan), cancellationToken);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(detail: result.Error.Message,
                    statusCode: result.Error.Code.Contains("NotFound") ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest,
                    title: result.Error.Code);
        })
        .WithName("ChangeTenantPlan")
        .WithSummary("Change tenant plan")
        .WithDescription("Changes the plan for a tenant. Clears trial period when upgrading to a paid plan.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPut("/tenants/{id:guid}/custom-domain", async (Guid id, SetCustomDomainRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new SetCustomDomainCommand(id, request.Domain), cancellationToken);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(detail: result.Error.Message,
                    statusCode: result.Error.Code.Contains("NotFound") ? StatusCodes.Status404NotFound
                              : result.Error.Code.Contains("AlreadyTaken") ? StatusCodes.Status409Conflict
                              : StatusCodes.Status400BadRequest,
                    title: result.Error.Code);
        })
        .WithName("SetTenantCustomDomain")
        .WithSummary("Set tenant custom domain")
        .WithDescription("Assigns a custom domain to a tenant. Pass null to remove. Used for white-label deployments.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}

public sealed record ChangePlanRequest(string Plan);
public sealed record SetCustomDomainRequest(string? Domain);
