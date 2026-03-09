using AgendeAqui.Api.Auth;
using AgendeAqui.Application.ApiKeys.CreateApiKey;
using AgendeAqui.Application.ApiKeys.ListApiKeys;
using AgendeAqui.Application.ApiKeys.RevokeApiKey;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AgendeAqui.Api.Endpoints;

public static class ApiKeyEndpoints
{
    public static void MapApiKeyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/api-keys")
            .WithTags("API Keys")
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .RequireRateLimiting("tenant");

        group.MapPost("/", async (
            [FromBody] CreateApiKeyRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new CreateApiKeyCommand(request.Name, request.ExpiresAt);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess
                ? Results.Created($"/api/v1/api-keys/{result.Value.Id}", result.Value)
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: result.Error.Code);
        })
        .WithName("CreateApiKey")
        .WithSummary("Create a new API key")
        .WithDescription("Generates a new API key. The raw key is returned only once and cannot be retrieved again.")
        .Produces<CreateApiKeyResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/", async (
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new ListApiKeysQuery(), ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: result.Error.Code);
        })
        .WithName("ListApiKeys")
        .WithSummary("List all API keys")
        .WithDescription("Returns all API keys for the current tenant with prefix and status.")
        .Produces<List<ApiKeyResponse>>(StatusCodes.Status200OK);

        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new RevokeApiKeyCommand(id), ct);

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
        .WithName("RevokeApiKey")
        .WithSummary("Revoke an API key")
        .WithDescription("Deactivates an API key so it can no longer be used for authentication.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private sealed record CreateApiKeyRequest(string Name, DateTime? ExpiresAt);
}
