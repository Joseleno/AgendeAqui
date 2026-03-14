using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Appointments.GetMyFilledSlots;

public sealed record GetMyFilledSlotsQuery(DateOnly Date) : IQuery<FilledSlotsResponse>;
