using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Reports.GetPlatformDashboard;

public sealed record GetPlatformDashboardQuery(DateOnly From, DateOnly To) : IQuery<PlatformDashboardResponse>;
