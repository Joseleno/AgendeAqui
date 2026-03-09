namespace AgendeAqui.Application.Reports.GetRevenueReport;

public sealed record RevenueReportResponse(
    decimal TotalRevenue,
    List<ServiceRevenue> ByService);

public sealed record ServiceRevenue(
    Guid ServiceId,
    string ServiceName,
    decimal UnitPrice,
    int AppointmentCount,
    decimal TotalRevenue);
