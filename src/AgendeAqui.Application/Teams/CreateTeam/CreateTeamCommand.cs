using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Teams.CreateTeam;

public sealed record CreateTeamCommand(string Name, string? Description) : ICommand<Guid>;
