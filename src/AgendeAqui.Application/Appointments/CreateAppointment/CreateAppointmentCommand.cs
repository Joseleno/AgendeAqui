using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Appointments.CreateAppointment;

public sealed record CreateAppointmentCommand(
    Guid ProfessionalId,
    Guid ServiceId,
    Guid ClientId,
    DateOnly Date,
    TimeOnly StartTime,
    string? Notes,
    string? ExternalId = null) : ICommand<Guid>;
