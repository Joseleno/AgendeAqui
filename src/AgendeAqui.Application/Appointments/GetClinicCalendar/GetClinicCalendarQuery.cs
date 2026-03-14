using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Appointments.GetClinicCalendar;

public sealed record GetClinicCalendarQuery(DateOnly Date) : IQuery<ClinicCalendarResponse>;
