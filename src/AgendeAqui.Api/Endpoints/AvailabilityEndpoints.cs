using AgendeAqui.Api.Auth;
using AgendeAqui.Application.Availability.GetAvailability;
using AgendeAqui.Application.Availability.GetAvailableProfessionals;
using Mediator;

namespace AgendeAqui.Api.Endpoints;

public static class AvailabilityEndpoints
{
    public static void MapAvailabilityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/availability")
            .WithTags("Availability")
            .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated)
            .RequireRateLimiting("tenant");

        group.MapGet("/", async (Guid professionalId, DateOnly date, Guid serviceId, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetAvailabilityQuery(professionalId, date, serviceId);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: result.Error.Code);
        })
        .WithName("GetAvailability")
        .WithSummary("Get availability")
        .WithDescription("Returns available time slots for a professional on a specific date.")
        .Produces<AvailabilityResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/professionals", async (Guid serviceId, DateOnly date, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var query = new GetAvailableProfessionalsQuery(serviceId, date);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(
                    detail: result.Error.Message,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: result.Error.Code);
        })
        .WithName("GetAvailableProfessionals")
        .WithSummary("Get available professionals for a service")
        .WithDescription("Returns professionals that can provide a service on a given date, with their available slot count.")
        .Produces<List<ProfessionalAvailabilityResponse>>()
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
