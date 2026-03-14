namespace AgendeAqui.Application.Reports.GetProfessionalRanking;

public sealed record ProfessionalRankingResponse(
    Guid ProfessionalId,
    string Name,
    string? Specialty,
    int Total,
    int Completed,
    int Cancelled,
    int NoShow,
    decimal CompletionRate,
    decimal NoShowRate);
