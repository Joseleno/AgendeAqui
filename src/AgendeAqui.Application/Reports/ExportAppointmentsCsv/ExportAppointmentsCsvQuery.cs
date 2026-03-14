using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Reports.ExportAppointmentsCsv;

public sealed record ExportAppointmentsCsvQuery(
    DateOnly From,
    DateOnly To,
    Guid? ProfessionalId = null) : IQuery<string>;
