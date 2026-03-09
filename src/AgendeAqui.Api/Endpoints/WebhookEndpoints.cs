using AgendeAqui.Api.Auth;
using AgendeAqui.Application.Webhooks.DeleteWebhook;
using AgendeAqui.Application.Webhooks.ListWebhooks;
using AgendeAqui.Application.Webhooks.RegisterWebhook;
using AgendeAqui.Application.Webhooks.UpdateWebhook;
using Mediator;

namespace AgendeAqui.Api.Endpoints;

public static class WebhookEndpoints
{
    public static void MapWebhookEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/webhooks")
            .WithTags("Webhooks")
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .RequireRateLimiting("tenant");

        group.MapPost("/", async (RegisterWebhookRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new RegisterWebhookCommand(request.Url, request.Secret, request.Events);
            var result = await mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return Results.Created($"/api/v1/webhooks/{result.Value}", new { id = result.Value });

            return Results.Problem(detail: result.Error.Message, statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code);
        })
        .WithName("RegisterWebhook")
        .WithSummary("Register webhook")
        .WithDescription("Registers a new webhook URL for event notifications.")
        .Produces(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/", async (IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new ListWebhooksQuery();
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(detail: result.Error.Message, statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code);
        })
        .WithName("ListWebhooks")
        .WithSummary("List webhooks")
        .WithDescription("Lists registered webhooks for the current tenant.")
        .Produces<List<WebhookResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", async (Guid id, UpdateWebhookRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new UpdateWebhookCommand(id, request.Url, request.Events, request.IsActive);
            var result = await mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return Results.NoContent();

            return Results.Problem(
                detail: result.Error.Message,
                statusCode: result.Error.IsNotFound
                    ? StatusCodes.Status404NotFound
                    : StatusCodes.Status400BadRequest,
                title: result.Error.Code);
        })
        .WithName("UpdateWebhook")
        .WithSummary("Update webhook")
        .WithDescription("Updates the URL, events, and active status of a registered webhook.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new DeleteWebhookCommand(id);
            var result = await mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return Results.NoContent();

            return Results.Problem(
                detail: result.Error.Message,
                statusCode: result.Error.IsNotFound
                    ? StatusCodes.Status404NotFound
                    : StatusCodes.Status400BadRequest,
                title: result.Error.Code);
        })
        .WithName("DeleteWebhook")
        .WithSummary("Delete webhook")
        .WithDescription("Deactivates a registered webhook.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}

public record RegisterWebhookRequest(string Url, string Secret, IReadOnlyList<string> Events);

public record UpdateWebhookRequest(string Url, IReadOnlyList<string> Events, bool IsActive);
