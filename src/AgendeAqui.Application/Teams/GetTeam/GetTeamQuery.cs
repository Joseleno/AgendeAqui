using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Teams.GetTeam;

public sealed record GetTeamQuery(Guid TeamId) : IQuery<TeamDetailResponse>;
