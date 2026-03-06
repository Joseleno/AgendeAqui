using AgendeAqui.Application.Availability.GetAvailability;
using Mediator;

namespace AgendeAqui.Api.Endpoints;

public static class AvailabilityEndpoints
{
    public static void MapAvailabilityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/availability")
            .WithTags("Availability");

        group.MapGet("/", async (Guid professionalId, DateOnly date, Guid serviceId, IMediator mediator) =>
        {
            var query = new GetAvailabilityQuery(professionalId, date, serviceId);
            var result = await mediator.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error.Message });
        })
        .WithName("GetAvailability")
        .Produces<AvailabilityResponse>()
        .Produces(StatusCodes.Status400BadRequest);
    }
}
