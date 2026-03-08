using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Reports.GetAttendanceReport;

public sealed record GetAttendanceReportQuery(DateOnly From, DateOnly To) : IQuery<AttendanceReportResponse>;
