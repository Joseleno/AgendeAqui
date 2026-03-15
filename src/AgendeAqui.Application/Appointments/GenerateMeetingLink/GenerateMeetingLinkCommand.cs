using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Appointments.GenerateMeetingLink;

public sealed record GenerateMeetingLinkCommand(Guid AppointmentId) : ICommand<string>;
