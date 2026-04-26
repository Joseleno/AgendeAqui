using AgendeAqui.Api.Auth;
using AgendeAqui.Application.Teams.AddTeamMember;
using AgendeAqui.Application.Teams.AssignTeamLeader;
using AgendeAqui.Application.Teams.CreateTeam;
using AgendeAqui.Application.Teams.DisbandTeam;
using AgendeAqui.Application.Teams.GetTeam;
using AgendeAqui.Application.Teams.ListTeams;
using AgendeAqui.Application.Teams.RemoveTeamMember;
using AgendeAqui.Application.Teams.UpdateTeam;
using Mediator;

namespace AgendeAqui.Api.Endpoints;

public static class TeamEndpoints
{
    public static void MapTeamEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/teams")
            .WithTags("Teams")
            .RequireAuthorization(AuthorizationPolicies.RequireAdmin)
            .RequireRateLimiting("tenant");

        group.MapPost("/", async (CreateTeamRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var command = new CreateTeamCommand(request.Name, request.Description);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess
                ? Results.Created($"/api/v1/teams/{result.Value}", new { id = result.Value })
                : Results.Problem(detail: result.Error.Message, statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code);
        })
        .WithName("CreateTeam")
        .WithSummary("Create team")
        .Produces(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new ListTeamsQuery(), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(detail: result.Error.Message, statusCode: StatusCodes.Status400BadRequest, title: result.Error.Code);
        })
        .WithName("ListTeams")
        .WithSummary("List teams")
        .Produces<List<TeamResponse>>(StatusCodes.Status200OK)
        .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetTeamQuery(id), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(detail: result.Error.Message,
                    statusCode: result.Error.IsNotFound ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest,
                    title: result.Error.Code);
        })
        .WithName("GetTeam")
        .WithSummary("Get team by ID")
        .Produces<TeamDetailResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization(AuthorizationPolicies.RequireAuthenticated);

        group.MapPut("/{id:guid}", async (Guid id, UpdateTeamRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new UpdateTeamCommand(id, request.Name, request.Description), ct);
            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(detail: result.Error.Message,
                    statusCode: result.Error.IsNotFound ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest,
                    title: result.Error.Code);
        })
        .WithName("UpdateTeam")
        .WithSummary("Update team")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new DisbandTeamCommand(id), ct);
            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(detail: result.Error.Message,
                    statusCode: result.Error.IsNotFound ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest,
                    title: result.Error.Code);
        })
        .WithName("DisbandTeam")
        .WithSummary("Disband team")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/members", async (Guid id, AddMemberRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new AddTeamMemberCommand(id, request.ProfessionalId), ct);
            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(detail: result.Error.Message,
                    statusCode: result.Error.IsNotFound ? StatusCodes.Status404NotFound : StatusCodes.Status409Conflict,
                    title: result.Error.Code);
        })
        .WithName("AddTeamMember")
        .WithSummary("Add member to team")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}/members/{professionalId:guid}", async (Guid id, Guid professionalId, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new RemoveTeamMemberCommand(id, professionalId), ct);
            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(detail: result.Error.Message,
                    statusCode: result.Error.IsNotFound ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest,
                    title: result.Error.Code);
        })
        .WithName("RemoveTeamMember")
        .WithSummary("Remove member from team")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}/leader", async (Guid id, AssignLeaderRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new AssignTeamLeaderCommand(id, request.ProfessionalId), ct);
            return result.IsSuccess
                ? Results.NoContent()
                : Results.Problem(detail: result.Error.Message,
                    statusCode: result.Error.IsNotFound ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest,
                    title: result.Error.Code);
        })
        .WithName("AssignTeamLeader")
        .WithSummary("Assign or clear team leader")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    private sealed record CreateTeamRequest(string Name, string? Description);
    private sealed record UpdateTeamRequest(string Name, string? Description);
    private sealed record AddMemberRequest(Guid ProfessionalId);
    private sealed record AssignLeaderRequest(Guid? ProfessionalId);
}
