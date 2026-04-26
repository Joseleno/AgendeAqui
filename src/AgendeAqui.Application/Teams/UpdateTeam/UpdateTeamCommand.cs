using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Teams.UpdateTeam;

public sealed record UpdateTeamCommand(Guid TeamId, string Name, string? Description) : ICommand;
