using AgendeAqui.Api.Auth;
using AgendeAqui.Api.Endpoints.Requests;
using AgendeAqui.Application.Common;
using AgendeAqui.Application.Payments.CreatePayment;
using AgendeAqui.Application.Payments.GetPaymentSummary;
using AgendeAqui.Application.Payments.ListPayments;
using AgendeAqui.Application.Payments.UpdatePaymentStatus;
using Mediator;

namespace AgendeAqui.Api.Endpoints;

public static class PaymentEndpoints
{
    public static void MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/payments")
            .WithTags("Payments")
            .RequireRateLimiting("tenant");

        group.MapPost("/", async (CreatePaymentRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new CreatePaymentCommand(
                request.AppointmentId,
                request.Amount,
                request.Method,
                request.Notes);

            var result = await mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return Results.Created($"/api/v1/payments/{result.Value}", new { id = result.Value });

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
        .WithName("CreatePayment")
        .WithSummary("Create payment")
        .WithDescription("Creates a new payment record for the specified appointment.")
        .Produces(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(AuthorizationPolicies.RequireProfessional);

        group.MapPut("/{id:guid}/status", async (Guid id, UpdatePaymentStatusRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = new UpdatePaymentStatusCommand(id, request.Status);
            var result = await mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return Results.NoContent();

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
        .WithName("UpdatePaymentStatus")
        .WithSummary("Update payment status")
        .WithDescription("Updates the status of an existing payment to Paid or Refunded.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(AuthorizationPolicies.RequireAdmin);

        group.MapGet("/", async (
            Guid? appointmentId, int? page, int? pageSize,
            DateOnly? from, DateOnly? to,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var query = new ListPaymentsQuery(
                appointmentId,
                Math.Max(1, page ?? 1),
                Math.Clamp(pageSize ?? 20, 1, 100),
                from,
                to);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: result.Error.Code);
        })
        .WithName("ListPayments")
        .WithSummary("List payments")
        .WithDescription("Lists payments with optional filter by appointment. Supports pagination.")
        .Produces<PagedResponse<PaymentResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .RequireAuthorization(AuthorizationPolicies.RequireProfessional);

        group.MapGet("/summary", async (
            DateOnly from, DateOnly to,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var query = new GetPaymentSummaryQuery(from, to);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: result.Error.Code);
        })
        .WithName("GetPaymentSummary")
        .WithSummary("Payment summary")
        .WithDescription("Returns aggregated payment statistics for the specified date range, including totals by method.")
        .Produces<PaymentSummaryResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .RequireAuthorization(AuthorizationPolicies.RequireAdmin);
    }
}
