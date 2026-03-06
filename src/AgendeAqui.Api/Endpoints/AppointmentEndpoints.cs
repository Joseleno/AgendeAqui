using AgendeAqui.Api.Endpoints.Requests;
using AgendeAqui.Application.Appointments.CancelAppointment;
using AgendeAqui.Application.Appointments.CreateAppointment;
using AgendeAqui.Application.Appointments.GetAppointment;
using AgendeAqui.Application.Appointments.ListAppointments;
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
            .WithTags("Appointments");

        group.MapPost("/", async (CreateAppointmentRequest request, IMediator mediator) =>
        {
            var command = new CreateAppointmentCommand(
                request.ProfessionalId,
                request.ServiceId,
                request.ClientId,
                request.Date,
                request.StartTime,
                request.Notes);

            var result = await mediator.Send(command);

            if (result.IsSuccess)
                return Results.Created($"/api/v1/appointments/{result.Value}", new { id = result.Value });

            if (result.Error.IsNotFound)
                return Results.NotFound(new { error = result.Error.Message });

            if (result.Error.IsConflict)
                return Results.Conflict(new { error = result.Error.Message });

            return Results.BadRequest(new { error = result.Error.Message });
        })
        .WithName("CreateAppointment")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var query = new GetAppointmentQuery(id);
            var result = await mediator.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { error = result.Error.Message });
        })
        .WithName("GetAppointment")
        .Produces<AppointmentResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", async (
            int? page, int? pageSize,
            DateOnly? dateFrom, DateOnly? dateTo,
            Guid? professionalId, string? status,
            IMediator mediator) =>
        {
            var query = new ListAppointmentsQuery(
                page ?? 1, pageSize ?? 10,
                dateFrom, dateTo, professionalId, status);

            var result = await mediator.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error.Message });
        })
        .WithName("ListAppointments")
        .Produces<PagedResponse<AppointmentResponse>>(StatusCodes.Status200OK);

        group.MapPost("/{id:guid}/cancel", async (Guid id, CancelAppointmentRequest request, IMediator mediator) =>
        {
            var command = new CancelAppointmentCommand(id, request.Reason);
            var result = await mediator.Send(command);

            if (result.IsSuccess)
                return Results.NoContent();

            return result.Error.IsNotFound
                ? Results.NotFound(new { error = result.Error.Message })
                : Results.BadRequest(new { error = result.Error.Message });
        })
        .WithName("CancelAppointment")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/reschedule", async (Guid id, RescheduleAppointmentRequest request, IMediator mediator) =>
        {
            var command = new RescheduleAppointmentCommand(id, request.NewDate, request.NewStartTime);
            var result = await mediator.Send(command);

            if (result.IsSuccess)
                return Results.NoContent();

            return result.Error.IsNotFound
                ? Results.NotFound(new { error = result.Error.Message })
                : Results.BadRequest(new { error = result.Error.Message });
        })
        .WithName("RescheduleAppointment")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/attendance", async (Guid id, UpdateAttendanceRequest request, IMediator mediator) =>
        {
            if (!Enum.TryParse<AttendanceAction>(request.Action, ignoreCase: false, out var action))
                return Results.BadRequest(new { error = "Action must be one of: Confirm, Start, Complete, NoShow." });

            var command = new UpdateAttendanceCommand(id, action);
            var result = await mediator.Send(command);

            if (result.IsSuccess)
                return Results.NoContent();

            return result.Error.IsNotFound
                ? Results.NotFound(new { error = result.Error.Message })
                : Results.BadRequest(new { error = result.Error.Message });
        })
        .WithName("UpdateAttendance")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}
