using AgendeAqui.Api.Auth;
using AgendeAqui.Application.Reports.GetPlatformDashboard;
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
    }
}
