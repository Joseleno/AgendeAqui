using AgendeAqui.Api.Auth;
using AgendeAqui.Api.Endpoints.Requests;
using AgendeAqui.Application.Appointments.CancelAppointment;
using AgendeAqui.Application.Appointments.CreateAppointment;
using AgendeAqui.Application.Appointments.GetAppointment;
using AgendeAqui.Application.Appointments.GetClinicCalendar;
using AgendeAqui.Application.Appointments.GetMyCalendar;
using AgendeAqui.Application.Appointments.GetMyFilledSlots;
using AgendeAqui.Application.Appointments.ListAppointments;
using AgendeAqui.Application.Appointments.ListMyAppointments;
using AgendeAqui.Application.Appointments.RescheduleAppointment;
using AgendeAqui.Application.Appointments.UpdateAttendance;
using AgendeAqui.Application.Common;
using Mediator;

namespace AgendeAqui.Api.Endpoints;

public static class AppointmentEndpoints
{
    public static void MapAppointmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/appointments")
            .WithTags("Appointments")
            .RequireRateLimiting("tenant");

        group.MapPost("/", async (CreateAppointmentRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new CreateAppointmentCommand(
                request.ProfessionalId,
                request.ServiceId,
                request.ClientId,
                request.Date,
                request.StartTime,
                request.Notes,
                request.ExternalId);

            var result = await mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return Results.Created($"/api/v1/appointments/{result.Value}", new { id = result.Value });

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

            if (result.Error.IsConflict)
                return Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status409Conflict,
                    title: result.Error.Code);

            return Results.Problem(
                detail: result.Error.Message,
                statusCode: StatusCodes.Status400BadRequest,
                title: result.Error.Code);
        })
        .WithName("CreateAppointment")
        .WithSummary("Create appointment")
        .WithDescription("Creates a new appointment for the specified professional, service, and client.")
        .Produces(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .RequireAuthorization(AuthorizationPolicies.RequireClient);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetAppointmentQuery(id);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status404NotFound,
                    title: result.Error.Code);
        })
        .WithName("GetAppointment")
        .WithSummary("Get appointment by ID")
        .WithDescription("Retrieves appointment details including professional, service, client, date, time, and status.")
        .Produces<AppointmentResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated);

        group.MapGet("/", async (
            int? page, int? pageSize,
            DateOnly? dateFrom, DateOnly? dateTo,
            Guid? professionalId, string? status, Guid? clientId, string? externalId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var query = new ListAppointmentsQuery(
                page ?? 1, pageSize ?? 10,
                dateFrom, dateTo, professionalId, status, clientId, externalId);

            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: result.Error.Code);
        })
        .WithName("ListAppointments")
        .WithSummary("List appointments")
        .WithDescription("Lists appointments with optional filters by date range, professional, and status. Supports pagination.")
        .Produces<PagedResponse<AppointmentResponse>>(StatusCodes.Status200OK)
        .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated);

        group.MapGet("/mine", async (
            int? page, int? pageSize, string? status,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var query = new ListMyAppointmentsQuery(page ?? 1, pageSize ?? 10, status);
            var result = await mediator.Send(query, cancellationToken);

            if (result.IsSuccess)
                return Results.Ok(result.Value);

            var statusCode = result.Error.IsNotAuthorized ? StatusCodes.Status403Forbidden
                : StatusCodes.Status400BadRequest;

            return Results.Problem(
                detail: result.Error.Message,
                statusCode: statusCode,
                title: result.Error.Code);
        })
        .WithName("ListMyAppointments")
        .WithSummary("List my appointments")
        .WithDescription("Lists the authenticated client's appointments with optional status filter. Supports pagination.")
        .Produces<PagedResponse<MyAppointmentResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated);

        group.MapPost("/{id:guid}/cancel", async (Guid id, CancelAppointmentRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new CancelAppointmentCommand(id, request.Reason);
            var result = await mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return Results.NoContent();

            var statusCode = result.Error.IsNotAuthorized ? StatusCodes.Status403Forbidden
                : result.Error.IsNotFound ? StatusCodes.Status404NotFound
                : result.Error.IsConflict ? StatusCodes.Status409Conflict
                : StatusCodes.Status400BadRequest;

            return Results.Problem(
                detail: result.Error.Message,
                statusCode: statusCode,
                title: result.Error.Code);
        })
        .WithName("CancelAppointment")
        .WithSummary("Cancel appointment")
        .WithDescription("Cancels an existing appointment with a required reason.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .RequireAuthorization(AuthorizationPolicies.RequireClient);

        group.MapPost("/{id:guid}/reschedule", async (Guid id, RescheduleAppointmentRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new RescheduleAppointmentCommand(id, request.NewDate, request.NewStartTime);
            var result = await mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return Results.NoContent();

            var statusCode = result.Error.IsNotAuthorized ? StatusCodes.Status403Forbidden
                : result.Error.IsNotFound ? StatusCodes.Status404NotFound
                : result.Error.IsConflict ? StatusCodes.Status409Conflict
                : StatusCodes.Status400BadRequest;

            return Results.Problem(
                detail: result.Error.Message,
                statusCode: statusCode,
                title: result.Error.Code);
        })
        .WithName("RescheduleAppointment")
        .WithSummary("Reschedule appointment")
        .WithDescription("Reschedules an appointment to a new date and time.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .RequireAuthorization(AuthorizationPolicies.RequireClient);

        group.MapPost("/{id:guid}/attendance", async (Guid id, UpdateAttendanceRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            if (!Enum.TryParse<AttendanceAction>(request.Action, ignoreCase: false, out var action))
                return Results.Problem(
                    detail: "Action must be one of: Confirm, Start, Complete, NoShow.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "InvalidAction");

            var command = new UpdateAttendanceCommand(id, action);
            var result = await mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return Results.NoContent();

            var statusCode = result.Error.IsNotFound ? StatusCodes.Status404NotFound
                : result.Error.IsConflict ? StatusCodes.Status409Conflict
                : StatusCodes.Status400BadRequest;

            return Results.Problem(
                detail: result.Error.Message,
                statusCode: statusCode,
                title: result.Error.Code);
        })
        .WithName("UpdateAttendance")
        .WithSummary("Update attendance")
        .WithDescription("Marks an appointment as attended or no-show.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .RequireAuthorization(AuthorizationPolicies.RequireProfessional);

        group.MapGet("/calendar", async (
            DateOnly? dateFrom,
            DateOnly? dateTo,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var query = new GetMyCalendarQuery(
                dateFrom ?? DateOnly.FromDateTime(DateTime.UtcNow),
                dateTo ?? DateOnly.FromDateTime(DateTime.UtcNow));

            var result = await mediator.Send(query, cancellationToken);

            if (result.IsSuccess)
                return Results.Ok(result.Value);

            var statusCode = result.Error.IsNotAuthorized
                ? StatusCodes.Status403Forbidden
                : StatusCodes.Status400BadRequest;

            return Results.Problem(
                detail: result.Error.Message,
                statusCode: statusCode,
                title: result.Error.Code);
        })
        .WithName("GetMyCalendar")
        .WithSummary("Get professional's calendar")
        .WithDescription("Returns the authenticated professional's appointments and absences grouped by day for the specified date range (max 31 days).")
        .Produces<CalendarResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .RequireAuthorization(AuthorizationPolicies.RequireProfessional);

        group.MapGet("/filled-slots", async (
            DateOnly? date,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var query = new GetMyFilledSlotsQuery(date ?? DateOnly.FromDateTime(DateTime.UtcNow));
            var result = await mediator.Send(query, cancellationToken);

            if (result.IsSuccess)
                return Results.Ok(result.Value);

            var statusCode = result.Error.IsNotAuthorized
                ? StatusCodes.Status403Forbidden
                : StatusCodes.Status400BadRequest;

            return Results.Problem(
                detail: result.Error.Message,
                statusCode: statusCode,
                title: result.Error.Code);
        })
        .WithName("GetMyFilledSlots")
        .WithSummary("Get professional's filled slots")
        .WithDescription("Returns the authenticated professional's filled appointment slots for a specific date, including total, filled, and available slot counts.")
        .Produces<FilledSlotsResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .RequireAuthorization(AuthorizationPolicies.RequireProfessional);

        group.MapGet("/clinic-calendar", async (
            DateOnly? date,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var query = new GetClinicCalendarQuery(date ?? DateOnly.FromDateTime(DateTime.UtcNow));
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: result.Error.Code);
        })
        .WithName("GetClinicCalendar")
        .WithSummary("Get clinic calendar")
        .WithDescription("Returns all appointments for every professional on the given date, grouped by professional.")
        .Produces<ClinicCalendarResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .RequireAuthorization(AuthorizationPolicies.RequireAdmin);
    }
}
