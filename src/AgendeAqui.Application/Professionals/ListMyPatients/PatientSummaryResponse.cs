namespace AgendeAqui.Application.Professionals.ListMyPatients;

public sealed record PatientSummaryResponse(
    Guid ClientId,
    string Name,
    string Email,
    string Phone,
    int TotalAppointments,
    DateOnly? LastVisit);
