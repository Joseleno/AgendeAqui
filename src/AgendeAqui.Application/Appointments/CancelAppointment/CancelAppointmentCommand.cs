using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Appointments.CancelAppointment;

public sealed record CancelAppointmentCommand(
    Guid AppointmentId,
    string Reason) : ICommand;
