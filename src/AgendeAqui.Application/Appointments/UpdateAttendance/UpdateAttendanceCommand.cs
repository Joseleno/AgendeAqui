using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Appointments.UpdateAttendance;

public sealed record UpdateAttendanceCommand(
    Guid AppointmentId,
    AttendanceAction Action) : ICommand;
