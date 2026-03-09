using FluentValidation;

namespace AgendeAqui.Application.Schedules.DeactivateSchedule;

public sealed class DeactivateScheduleCommandValidator : AbstractValidator<DeactivateScheduleCommand>
{
    public DeactivateScheduleCommandValidator()
    {
        RuleFor(x => x.ScheduleId)
            .NotEmpty();
    }
}
