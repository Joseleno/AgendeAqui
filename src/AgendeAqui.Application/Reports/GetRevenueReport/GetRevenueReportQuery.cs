using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Reports.GetRevenueReport;

public sealed record GetRevenueReportQuery(DateOnly From, DateOnly To) : IQuery<RevenueReportResponse>;
