using FluentValidation;

namespace AgendeAqui.Application.InAppNotifications.MarkAllAsRead;

public sealed class MarkAllAsReadCommandValidator : AbstractValidator<MarkAllAsReadCommand>
{
    public MarkAllAsReadCommandValidator()
    {
    }
}
