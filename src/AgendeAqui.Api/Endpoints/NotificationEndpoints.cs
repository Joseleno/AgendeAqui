using AgendeAqui.Api.Auth;
using AgendeAqui.Application.Notifications.ListNotifications;
using Mediator;

namespace AgendeAqui.Api.Endpoints;

public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/notifications")
            .WithTags("Notifications")
            .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated)
            .RequireRateLimiting("tenant");

        group.MapGet("/", async (
            Guid? appointmentId,
            DateOnly? dateFrom,
            DateOnly? dateTo,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var query = new ListNotificationsQuery(appointmentId, dateFrom, dateTo);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: result.Error.Code);
        })
        .WithName("ListNotifications")
        .WithSummary("List notifications")
        .WithDescription("Lists notifications with optional filters by appointment, date range. Includes tenant isolation.")
        .Produces<List<NotificationResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
