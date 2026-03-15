namespace AgendeAqui.Application.Reports.GetPatientGrowth;

public sealed record PatientGrowthResponse(List<GrowthPoint> Points);

public sealed record GrowthPoint(string Month, int NewClients, int TotalClients);
