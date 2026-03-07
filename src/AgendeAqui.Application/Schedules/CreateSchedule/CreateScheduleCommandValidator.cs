using FluentValidation;

namespace AgendeAqui.Application.Schedules.CreateSchedule;

public sealed class CreateScheduleCommandValidator : AbstractValidator<CreateScheduleCommand>
{
    public CreateScheduleCommandValidator()
    {
        RuleFor(x => x.ProfessionalId)
            .NotEmpty();

        RuleFor(x => x.DayOfWeek)
            .IsInEnum()
            .WithMessage("DayOfWeek must be between 0 (Sunday) and 6 (Saturday).");

        RuleFor(x => x.SlotDurationMinutes)
            .InclusiveBetween(1, 720)
            .WithMessage("Slot duration must be between 1 and 720 minutes.");

        RuleFor(x => x.StartTime)
            .LessThan(x => x.EndTime)
            .WithMessage("Start time must be before end time.");
    }
}
