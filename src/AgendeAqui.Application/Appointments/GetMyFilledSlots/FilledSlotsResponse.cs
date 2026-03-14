namespace AgendeAqui.Application.Appointments.GetMyFilledSlots;

public sealed record FilledSlotsResponse(
    DateOnly Date,
    int TotalSlots,
    int FilledSlots,
    int AvailableSlots,
    IReadOnlyList<FilledSlotResponse> Slots);

public sealed record FilledSlotResponse(
    string StartTime,
    string EndTime,
    string ClientName,
    string ServiceName,
    string Status);
