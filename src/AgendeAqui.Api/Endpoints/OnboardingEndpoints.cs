using AgendeAqui.Application.Tenants.RegisterTenant;
using Mediator;

namespace AgendeAqui.Api.Endpoints;

public static class OnboardingEndpoints
{
    public static void MapOnboardingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/onboarding")
            .WithTags("Onboarding")
            .RequireRateLimiting("tenant");

        group.MapPost("/register", async (RegisterTenantRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new RegisterTenantCommand(
                request.Name,
                request.Slug,
                request.AdminEmail,
                request.AdminPassword,
                request.AdminName);

            var result = await mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                return Results.Created($"/api/v1/tenants/{result.Value.TenantId}", new
                {
                    result.Value.TenantId,
                    result.Value.AdminUserId,
                    result.Value.TrialEndsAt,
                    Message = $"Trial started. Your account is active until {result.Value.TrialEndsAt:yyyy-MM-dd}."
                });
            }

            return Results.Problem(
                detail: result.Error.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: result.Error.Code);
        })
        .AllowAnonymous()
        .WithName("RegisterTenant")
        .WithSummary("Self-service tenant registration")
        .WithDescription("Creates a new tenant with a 14-day free trial and an Admin user. No authentication required.")
        .Produces(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}

public sealed record RegisterTenantRequest(
    string Name,
    string Slug,
    string AdminEmail,
    string AdminPassword,
    string AdminName);
