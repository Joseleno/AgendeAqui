namespace AgendeAqui.Application.Clients.ExportClientData;

public sealed record ClientDataExport(
    Guid Id,
    string Name,
    string Email,
    string Phone,
    DateTime CreatedAt,
    List<ClientAppointmentExport> Appointments);

public sealed record ClientAppointmentExport(
    Guid Id,
    DateOnly Date,
    string StartTime,
    string EndTime,
    string Status,
    DateTime CreatedAt);
