using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Reports.GetTeamAppointmentsReport;

public sealed record GetTeamAppointmentsReportQuery(DateOnly From, DateOnly To) : IQuery<TeamAppointmentsReportResponse>;
