using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Teams.ListTeams;

public sealed record ListTeamsQuery : IQuery<List<TeamResponse>>;
