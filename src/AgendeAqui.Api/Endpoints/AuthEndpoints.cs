using AgendeAqui.Application.Auth.Login;
using AgendeAqui.Application.Auth.Register;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Users;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AgendeAqui.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .WithTags("Auth");

        group.MapGet("/tenant/{slug}", async (string slug, ITenantRepository tenantRepository, CancellationToken ct) =>
        {
            var tenant = await tenantRepository.GetBySlugAsync(slug, ct);
            return tenant is null
                ? Results.NotFound()
                : Results.Ok(new { tenant.Id, tenant.Name });
        })
        .AllowAnonymous()
        .WithName("ResolveTenantBySlug")
        .WithSummary("Resolve tenant by slug")
        .WithDescription("Public endpoint to resolve a tenant slug to its ID and name. Used by the frontend before login/register.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new LoginCommand(request.Email, request.Password);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    title: result.Error.Code,
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status401Unauthorized);
        })
        .AllowAnonymous()
        .WithName("Login")
        .WithSummary("Authenticate with email and password")
        .WithDescription("Returns a JWT access token and refresh token for authenticated users.")
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/register", async (
            [FromBody] RegisterRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new RegisterPatientCommand(
                request.Name, request.Email, request.Phone, request.Password);

            var result = await mediator.Send(command, ct);

            if (result.IsSuccess)
                return Results.Created("/api/v1/auth/register", result.Value);

            if (result.Error == UserErrors.EmailAlreadyExists)
                return Results.Conflict(new ProblemDetails
                {
                    Title = result.Error.Code,
                    Detail = result.Error.Message,
                    Status = StatusCodes.Status409Conflict
                });

            return Results.Problem(
                title: result.Error.Code,
                detail: result.Error.Message,
                statusCode: StatusCodes.Status400BadRequest);
        })
        .AllowAnonymous()
        .WithName("RegisterPatient")
        .WithSummary("Self-register as a patient (client)")
        .WithDescription("Creates a new client and user account, returning a JWT access token. Requires X-Tenant-Id header.")
        .Produces<LoginResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status409Conflict)
        .Produces(StatusCodes.Status400BadRequest);
    }

    private sealed record LoginRequest(string Email, string Password);
    private sealed record RegisterRequest(string Name, string Email, string Phone, string Password);
}
