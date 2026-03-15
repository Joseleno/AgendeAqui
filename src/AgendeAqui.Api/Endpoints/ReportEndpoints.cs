using System.Text;
using AgendeAqui.Api.Auth;
using AgendeAqui.Application.Reports.ExportAppointmentsCsv;
using AgendeAqui.Application.Reports.GetAttendanceReport;
using AgendeAqui.Application.Reports.GetDashboardOverview;
using AgendeAqui.Application.Reports.GetMyStats;
using AgendeAqui.Application.Reports.GetAppointmentsByStatus;
using AgendeAqui.Application.Reports.GetAppointmentsTimeline;
using AgendeAqui.Application.Reports.GetBusiestHours;
using AgendeAqui.Application.Reports.GetPatientGrowth;
using AgendeAqui.Application.Reports.GetProfessionalRanking;
using AgendeAqui.Application.Reports.GetRevenueReport;
using AgendeAqui.Application.Reports.GetRevenueTimeline;
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

        group.MapGet("/attendance", async (DateOnly from, DateOnly to, Guid? professionalId, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetAttendanceReportQuery(from, to, professionalId);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(detail: result.Error.Message, statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code);
        })
        .WithName("GetAttendanceReport")
        .WithSummary("Attendance report")
        .WithDescription("Generates an attendance report for the specified date range. Optionally filtered by professionalId.")
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

        group.MapGet("/dashboard", async (IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetDashboardOverviewQuery();
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(detail: result.Error.Message, statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code);
        })
        .WithName("GetDashboardOverview")
        .WithSummary("Dashboard overview")
        .WithDescription("Returns key metrics for today and the current week for the admin dashboard.")
        .Produces<DashboardOverviewResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/professional-ranking", async (DateOnly from, DateOnly to, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetProfessionalRankingQuery(from, to);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(detail: result.Error.Message, statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code);
        })
        .WithName("GetProfessionalRanking")
        .WithSummary("Professional ranking")
        .WithDescription("Returns professionals ranked by completed appointments within the specified date range.")
        .Produces<List<ProfessionalRankingResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/export/appointments", async (DateOnly from, DateOnly to, Guid? professionalId, HttpContext httpContext, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new ExportAppointmentsCsvQuery(from, to, professionalId);
            var result = await mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
                return Results.Problem(detail: result.Error.Message, statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code);

            httpContext.Response.Headers.ContentDisposition = "attachment; filename=agendamentos.csv";
            return Results.Text(result.Value, "text/csv", Encoding.UTF8);
        })
        .WithName("ExportAppointmentsCsv")
        .WithSummary("Export appointments CSV")
        .WithDescription("Exports appointments as a semicolon-delimited CSV file for the specified date range. Optionally filtered by professionalId.")
        .Produces<string>(StatusCodes.Status200OK, "text/csv")
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/appointments-by-status", async (DateOnly from, DateOnly to, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetAppointmentsByStatusQuery(from, to);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(detail: result.Error.Message, statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code);
        })
        .WithName("GetAppointmentsByStatus")
        .WithSummary("Appointments by status")
        .WithDescription("Returns appointment counts grouped by status within the specified date range. Useful for donut charts.")
        .Produces<AppointmentsByStatusResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/appointments-timeline", async (DateOnly from, DateOnly to, string? groupBy, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetAppointmentsTimelineQuery(from, to, groupBy ?? "day");
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(detail: result.Error.Message, statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code);
        })
        .WithName("GetAppointmentsTimeline")
        .WithSummary("Appointments timeline")
        .WithDescription("Returns appointment counts by status grouped by day, week, or month. Useful for area charts.")
        .Produces<AppointmentsTimelineResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/busiest-hours", async (DateOnly from, DateOnly to, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetBusiestHoursQuery(from, to);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(detail: result.Error.Message, statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code);
        })
        .WithName("GetBusiestHours")
        .WithSummary("Busiest hours")
        .WithDescription("Returns appointment counts by day of week and hour. Useful for heatmap visualizations.")
        .Produces<BusiestHoursResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/patient-growth", async (int? months, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetPatientGrowthQuery(months ?? 12);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(detail: result.Error.Message, statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code);
        })
        .WithName("GetPatientGrowth")
        .WithSummary("Patient growth")
        .WithDescription("Returns new and cumulative client counts by month. Useful for line charts.")
        .Produces<PatientGrowthResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/revenue-timeline", async (DateOnly from, DateOnly to, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetRevenueTimelineQuery(from, to);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(detail: result.Error.Message, statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code);
        })
        .WithName("GetRevenueTimeline")
        .WithSummary("Revenue timeline")
        .WithDescription("Returns monthly revenue and appointment counts for completed appointments. Useful for bar charts.")
        .Produces<RevenueTimelineResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
