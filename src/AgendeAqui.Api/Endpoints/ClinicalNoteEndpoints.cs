using AgendeAqui.Api.Auth;
using AgendeAqui.Api.Endpoints.Requests;
using AgendeAqui.Application.ClinicalNotes.CreateClinicalNote;
using AgendeAqui.Application.ClinicalNotes.DeleteClinicalNote;
using AgendeAqui.Application.ClinicalNotes.GetClinicalNote;
using AgendeAqui.Application.ClinicalNotes.ListClinicalNotes;
using AgendeAqui.Application.ClinicalNotes.UpdateClinicalNote;
using AgendeAqui.Application.Common;
using Mediator;

namespace AgendeAqui.Api.Endpoints;

public static class ClinicalNoteEndpoints
{
    public static void MapClinicalNoteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/clinical-notes")
            .WithTags("ClinicalNotes")
            .RequireRateLimiting("tenant");

        group.MapPost("/", async (CreateClinicalNoteRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new CreateClinicalNoteCommand(
                request.ClientId,
                request.AppointmentId,
                request.Title,
                request.Content,
                request.IsPrivate);

            var result = await mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return Results.Created($"/api/v1/clinical-notes/{result.Value}", new { id = result.Value });

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
        .WithName("CreateClinicalNote")
        .WithSummary("Create clinical note")
        .WithDescription("Creates a new clinical note for a client. The authenticated professional is set as the author.")
        .Produces(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(AuthorizationPolicies.RequireProfessional);

        group.MapPut("/{id:guid}", async (Guid id, UpdateClinicalNoteRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new UpdateClinicalNoteCommand(id, request.Title, request.Content, request.IsPrivate);
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
        .WithName("UpdateClinicalNote")
        .WithSummary("Update clinical note")
        .WithDescription("Updates an existing clinical note. Only the author professional can edit.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(AuthorizationPolicies.RequireProfessional);

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new DeleteClinicalNoteCommand(id);
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
        .WithName("DeleteClinicalNote")
        .WithSummary("Delete clinical note")
        .WithDescription("Deletes a clinical note. Only the author professional can delete.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(AuthorizationPolicies.RequireProfessional);

        group.MapGet("/", async (
            Guid clientId,
            int? page, int? pageSize,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var query = new ListClinicalNotesQuery(
                clientId,
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
        .WithName("ListClinicalNotes")
        .WithSummary("List clinical notes")
        .WithDescription("Lists clinical notes for a client. Professionals see their own private notes and all non-private notes. Admins see everything.")
        .Produces<PagedResponse<ClinicalNoteResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetClinicalNoteQuery(id);
            var result = await mediator.Send(query, cancellationToken);

            if (result.IsSuccess)
                return Results.Ok(result.Value);

            return Results.Problem(
                detail: result.Error.Message,
                statusCode: result.Error.IsNotFound
                    ? StatusCodes.Status404NotFound
                    : StatusCodes.Status400BadRequest,
                title: result.Error.Code);
        })
        .WithName("GetClinicalNote")
        .WithSummary("Get clinical note by ID")
        .WithDescription("Retrieves a single clinical note. Private notes are only visible to the author professional and admins.")
        .Produces<ClinicalNoteResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated);
    }
}
