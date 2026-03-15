using AgendeAqui.Api.Auth;
using AgendeAqui.Application.Common;
using AgendeAqui.Application.InAppNotifications;
using AgendeAqui.Application.InAppNotifications.GetUnreadCount;
using AgendeAqui.Application.InAppNotifications.ListMyNotifications;
using AgendeAqui.Application.InAppNotifications.MarkAllAsRead;
using AgendeAqui.Application.InAppNotifications.MarkAsRead;
using Mediator;

namespace AgendeAqui.Api.Endpoints;

public static class InAppNotificationEndpoints
{
    public static void MapInAppNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/notifications/in-app")
            .WithTags("InAppNotifications")
            .RequireRateLimiting("tenant");

        group.MapGet("/", async (
            int? page, int? pageSize,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var query = new ListMyNotificationsQuery(
                Math.Max(1, page ?? 1),
                Math.Clamp(pageSize ?? 20, 1, 100));
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: result.Error.Code);
        })
        .WithName("ListMyNotifications")
        .WithSummary("List my notifications")
        .WithDescription("Lists in-app notifications for the authenticated user, ordered by most recent first.")
        .Produces<PagedResponse<InAppNotificationResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated);

        group.MapGet("/unread-count", async (
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var query = new GetUnreadCountQuery();
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(new { count = result.Value })
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: result.Error.Code);
        })
        .WithName("GetUnreadNotificationCount")
        .WithSummary("Get unread notification count")
        .WithDescription("Returns the number of unread in-app notifications for the authenticated user.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated);

        group.MapPut("/{id:guid}/read", async (
            Guid id,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new MarkAsReadCommand(id);
            var result = await mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return Results.NoContent();

            if (result.Error.IsNotAuthorized)
                return Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status403Forbidden,
                    title: result.Error.Code);

            if (result.Error.IsNotFound)
                return Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status404NotFound,
                    title: result.Error.Code);

            return Results.Problem(
                detail: result.Error.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: result.Error.Code);
        })
        .WithName("MarkNotificationAsRead")
        .WithSummary("Mark notification as read")
        .WithDescription("Marks a single in-app notification as read. Only the notification owner can mark it.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated);

        group.MapPut("/read-all", async (
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new MarkAllAsReadCommand();
            var result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: result.Error.Code);
        })
        .WithName("MarkAllNotificationsAsRead")
        .WithSummary("Mark all notifications as read")
        .WithDescription("Marks all unread in-app notifications as read for the authenticated user.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated);
    }
}
