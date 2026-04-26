using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Teams.AssignTeamLeader;

public sealed record AssignTeamLeaderCommand(Guid TeamId, Guid? ProfessionalId) : ICommand;
