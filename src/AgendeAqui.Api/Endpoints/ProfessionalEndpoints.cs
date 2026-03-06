using AgendeAqui.Application.Professionals.CreateProfessional;
using AgendeAqui.Application.Professionals.GetProfessional;
using AgendeAqui.Application.Professionals.ListProfessionals;
using AgendeAqui.Application.Professionals.UpdateProfessional;
using Mediator;

namespace AgendeAqui.Api.Endpoints;

public static class ProfessionalEndpoints
{
    public static void MapProfessionalEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/professionals")
            .WithTags("Professionals");

        group.MapPost("/", async (CreateProfessionalRequest request, IMediator mediator) =>
        {
            var command = new CreateProfessionalCommand(request.Name, request.Email, request.Phone);
            var result = await mediator.Send(command);

            return result.IsSuccess
                ? Results.Created($"/api/v1/professionals/{result.Value}", new { id = result.Value })
                : Results.BadRequest(new { error = result.Error.Message });
        })
        .WithName("CreateProfessional")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var query = new GetProfessionalQuery(id);
            var result = await mediator.Send(query);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { error = result.Error.Message });
        })
        .WithName("GetProfessional")
        .Produces<ProfessionalResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", async (int? page, int? pageSize, IMediator mediator) =>
        {
            var query = new ListProfessionalsQuery(page ?? 1, pageSize ?? 10);
            var result = await mediator.Send(query);

            return Results.Ok(result.Value);
        })
        .WithName("ListProfessionals")
        .Produces(StatusCodes.Status200OK);

        group.MapPut("/{id:guid}", async (Guid id, UpdateProfessionalRequest request, IMediator mediator) =>
        {
            var command = new UpdateProfessionalCommand(id, request.Name, request.Email, request.Phone);
            var result = await mediator.Send(command);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(new { error = result.Error.Message });
        })
        .WithName("UpdateProfessional")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

public sealed record CreateProfessionalRequest(string Name, string Email, string Phone);
public sealed record UpdateProfessionalRequest(string Name, string Email, string Phone);
