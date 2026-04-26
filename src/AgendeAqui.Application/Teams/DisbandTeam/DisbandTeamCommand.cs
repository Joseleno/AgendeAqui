using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Teams.DisbandTeam;

public sealed record DisbandTeamCommand(Guid TeamId) : ICommand;
