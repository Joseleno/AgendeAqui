using FluentValidation;

namespace AgendeAqui.Application.InAppNotifications.MarkAsRead;

public sealed class MarkAsReadCommandValidator : AbstractValidator<MarkAsReadCommand>
{
    public MarkAsReadCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
