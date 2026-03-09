using AgendeAqui.Application.Auth.Login;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AgendeAqui.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .WithTags("Auth");

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
    }

    private sealed record LoginRequest(string Email, string Password);
}
