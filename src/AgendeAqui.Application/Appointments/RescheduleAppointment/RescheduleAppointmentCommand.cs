using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Appointments.RescheduleAppointment;

public sealed record RescheduleAppointmentCommand(
    Guid AppointmentId,
    DateOnly NewDate,
    TimeOnly NewStartTime) : ICommand;
