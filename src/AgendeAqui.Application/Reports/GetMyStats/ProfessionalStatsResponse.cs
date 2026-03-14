namespace AgendeAqui.Application.Reports.GetMyStats;

public sealed record ProfessionalStatsResponse(
    int Total,
    int Completed,
    int Cancelled,
    int NoShow,
    int Scheduled,
    decimal CompletionRate);
