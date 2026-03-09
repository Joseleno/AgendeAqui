using AgendeAqui.Api.Auth;
using AgendeAqui.Application.Reports.GetAttendanceReport;
using AgendeAqui.Application.Reports.GetRevenueReport;
using Mediator;

namespace AgendeAqui.Api.Endpoints;

public static class ReportEndpoints
{
    public static void MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/reports")
            .WithTags("Reports")
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .RequireRateLimiting("tenant");

        group.MapGet("/attendance", async (DateOnly from, DateOnly to, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetAttendanceReportQuery(from, to);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(detail: result.Error.Message, statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code);
        })
        .WithName("GetAttendanceReport")
        .WithSummary("Attendance report")
        .WithDescription("Generates an attendance report for the specified date range.")
        .Produces<AttendanceReportResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/revenue", async (DateOnly from, DateOnly to, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetRevenueReportQuery(from, to);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(detail: result.Error.Message, statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code);
        })
        .WithName("GetRevenueReport")
        .WithSummary("Revenue report")
        .WithDescription("Generates a revenue report for the specified date range.")
        .Produces<RevenueReportResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
