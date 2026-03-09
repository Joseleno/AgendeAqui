using FluentValidation;

namespace AgendeAqui.Application.Schedules.UpdateSchedule;

public sealed class UpdateScheduleCommandValidator : AbstractValidator<UpdateScheduleCommand>
{
    public UpdateScheduleCommandValidator()
    {
        RuleFor(x => x.ScheduleId)
            .NotEmpty();

        RuleFor(x => x.SlotDurationMinutes)
            .InclusiveBetween(1, 720)
            .WithMessage("Slot duration must be between 1 and 720 minutes.");

        RuleFor(x => x.StartTime)
            .LessThan(x => x.EndTime)
            .WithMessage("Start time must be before end time.");
    }
}
