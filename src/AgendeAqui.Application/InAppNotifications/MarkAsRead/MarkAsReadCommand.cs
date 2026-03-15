using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.InAppNotifications.MarkAsRead;

public sealed record MarkAsReadCommand(Guid Id) : ICommand;
