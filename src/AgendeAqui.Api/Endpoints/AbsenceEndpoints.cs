using AgendeAqui.Api.Auth;
using AgendeAqui.Application.Absences.CreateAbsence;
using AgendeAqui.Application.Absences.DeleteAbsence;
using AgendeAqui.Application.Absences.ListAbsences;
using Mediator;

namespace AgendeAqui.Api.Endpoints;

public static class AbsenceEndpoints
{
    public static void MapAbsenceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/absences")
            .WithTags("Absences")
            .RequireRateLimiting("tenant");

        group.MapPost("/", async (CreateAbsenceRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var command = new CreateAbsenceCommand(
                request.ProfessionalId,
                request.Date,
                request.StartTime,
                request.EndTime,
                request.Reason);

            var result = await mediator.Send(command, ct);

            if (result.IsSuccess)
                return Results.Created($"/api/v1/absences/{result.Value}", new { id = result.Value });

            var statusCode = result.Error.IsNotFound
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return Results.Problem(
                detail: result.Error.Message,
                statusCode: statusCode,
                title: result.Error.Code);
        })
        .WithName("CreateAbsence")
        .WithSummary("Create absence/block")
        .WithDescription("Creates an absence or block period for a professional. Omit start/end times for a full-day absence.")
        .Produces(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(AuthorizationPolicies.RequireProfessional);

        group.MapGet("/", async (Guid professionalId, DateOnly from, DateOnly to, IMediator mediator, CancellationToken ct) =>
        {
            var query = new ListAbsencesQuery(professionalId, from, to);
            var result = await mediator.Send(query, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: result.Error.Code);
        })
        .WithName("ListAbsences")
        .WithSummary("List absences")
        .WithDescription("Lists absences for a professional within a date range.")
        .Produces<IReadOnlyList<AbsenceResponse>>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated);

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var command = new DeleteAbsenceCommand(id);
            var result = await mediator.Send(command, ct);

            if (result.IsSuccess)
                return Results.NoContent();

            return Results.Problem(
                detail: result.Error.Message,
                statusCode: result.Error.IsNotFound ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest,
                title: result.Error.Code);
        })
        .WithName("DeleteAbsence")
        .WithSummary("Delete absence")
        .WithDescription("Removes an absence/block period.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(AuthorizationPolicies.RequireProfessional);
    }

    private sealed record CreateAbsenceRequest(
        Guid ProfessionalId,
        DateOnly Date,
        TimeOnly? StartTime,
        TimeOnly? EndTime,
        string? Reason);
}
