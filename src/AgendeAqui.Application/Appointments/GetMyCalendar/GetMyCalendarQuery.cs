using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Appointments.GetMyCalendar;

public sealed record GetMyCalendarQuery(
    DateOnly DateFrom,
    DateOnly DateTo) : IQuery<CalendarResponse>;
