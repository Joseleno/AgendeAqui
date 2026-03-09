using FluentValidation;

namespace AgendeAqui.Application.Notifications.ListNotifications;

public sealed class ListNotificationsQueryValidator : AbstractValidator<ListNotificationsQuery>
{
    public ListNotificationsQueryValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty();
    }
}
