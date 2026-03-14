using FluentValidation;

namespace AgendeAqui.Application.Schedules.DetectConflicts;

public sealed class DetectScheduleConflictsQueryValidator : AbstractValidator<DetectScheduleConflictsQuery>
{
    public DetectScheduleConflictsQueryValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("'Date' is required.");
    }
}
