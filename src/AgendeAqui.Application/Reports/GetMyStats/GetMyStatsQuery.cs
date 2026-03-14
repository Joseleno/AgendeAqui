using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Reports.GetMyStats;

public sealed record GetMyStatsQuery(
    DateOnly From,
    DateOnly To) : IQuery<ProfessionalStatsResponse>;
