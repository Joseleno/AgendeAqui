using AgendeAqui.Api.Auth;
using AgendeAqui.Api.Endpoints.Requests;
using AgendeAqui.Application.Common;
using AgendeAqui.Application.Schedules.CreateSchedule;
using AgendeAqui.Application.Schedules.DeactivateSchedule;
using AgendeAqui.Application.Schedules.GetSchedule;
using AgendeAqui.Application.Schedules.ListSchedules;
using AgendeAqui.Application.Schedules.UpdateSchedule;
using Mediator;

namespace AgendeAqui.Api.Endpoints;

public static class ScheduleEndpoints
{
    public static void MapScheduleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/schedules")
            .WithTags("Schedules")
            .RequireRateLimiting("tenant");

        group.MapPost("/", async (CreateScheduleRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new CreateScheduleCommand(
                request.ProfessionalId,
                request.DayOfWeek,
                request.StartTime,
                request.EndTime,
                request.SlotDurationMinutes);

            var result = await mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return Results.Created($"/api/v1/schedules/{result.Value}", new { id = result.Value });

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
        .WithName("CreateSchedule")
        .WithSummary("Create schedule")
        .WithDescription("Creates a weekly schedule for a professional.")
        .Produces(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(AuthorizationPolicies.RequireProfessional);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetScheduleQuery(id);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status404NotFound,
                    title: result.Error.Code);
        })
        .WithName("GetSchedule")
        .WithSummary("Get schedule by ID")
        .WithDescription("Retrieves schedule details.")
        .Produces<ScheduleResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated);

        group.MapGet("/", async (
            int? page, int? pageSize,
            Guid? professionalId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var query = new ListSchedulesQuery(page ?? 1, pageSize ?? 10, professionalId);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: result.Error.Code);
        })
        .WithName("ListSchedules")
        .WithSummary("List schedules")
        .WithDescription("Lists schedules with optional professional filter. Supports pagination.")
        .Produces<PagedResponse<ScheduleResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated);

        group.MapPut("/{id:guid}", async (Guid id, UpdateScheduleRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new UpdateScheduleCommand(
                id,
                request.StartTime,
                request.EndTime,
                request.SlotDurationMinutes);

            var result = await mediator.Send(command, cancellationToken);

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
        .WithName("UpdateSchedule")
        .WithSummary("Update schedule")
        .WithDescription("Updates schedule times and active status.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(AuthorizationPolicies.RequireProfessional);

        group.MapPost("/{id:guid}/deactivate", async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new DeactivateScheduleCommand(id);
            var result = await mediator.Send(command, cancellationToken);

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
        .WithName("DeactivateSchedule")
        .WithSummary("Deactivate schedule")
        .WithDescription("Deactivates a professional's schedule.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(AuthorizationPolicies.RequireProfessional);
    }
}
