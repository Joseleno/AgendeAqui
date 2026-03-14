using AgendeAqui.Api.Auth;
using AgendeAqui.Application.Reports.GetAttendanceReport;
using AgendeAqui.Application.Reports.GetMyStats;
using AgendeAqui.Application.Reports.GetRevenueReport;
using Mediator;

namespace AgendeAqui.Api.Endpoints;

public static class ReportEndpoints
{
    public static void MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        var professionalGroup = app.MapGroup("/api/v1/reports")
            .WithTags("Reports")
            .RequireRateLimiting("tenant");

        professionalGroup.MapGet("/my-stats", async (DateOnly from, DateOnly to, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetMyStatsQuery(from, to);
            var result = await mediator.Send(query, cancellationToken);

            if (result.IsSuccess)
                return Results.Ok(result.Value);

            var statusCode = result.Error.IsNotFound ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

            return Results.Problem(
                detail: result.Error.Message,
                statusCode: statusCode,
                title: result.Error.Code);
        })
        .WithName("GetMyStats")
        .WithSummary("Get my stats")
        .WithDescription("Returns appointment statistics for the authenticated professional within the specified date range.")
        .Produces<ProfessionalStatsResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(AuthorizationPolicies.RequireProfessional);

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
