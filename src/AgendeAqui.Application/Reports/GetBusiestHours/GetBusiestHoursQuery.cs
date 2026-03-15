using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Reports.GetBusiestHours;

public sealed record GetBusiestHoursQuery(DateOnly From, DateOnly To) : IQuery<BusiestHoursResponse>;
