using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Reports.GetPatientGrowth;

public sealed record GetPatientGrowthQuery(int Months = 12) : IQuery<PatientGrowthResponse>;
