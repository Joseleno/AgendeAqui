using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Reports.GetProfessionalRanking;

public sealed record GetProfessionalRankingQuery(
    DateOnly From,
    DateOnly To) : IQuery<List<ProfessionalRankingResponse>>;
