using FluentValidation;

namespace AgendeAqui.Application.Appointments.GetMyCalendar;

public sealed class GetMyCalendarQueryValidator : AbstractValidator<GetMyCalendarQuery>
{
    public GetMyCalendarQueryValidator()
    {
        RuleFor(x => x.DateFrom)
            .NotEmpty()
            .WithMessage("'DateFrom' is required.");

        RuleFor(x => x.DateTo)
            .NotEmpty()
            .WithMessage("'DateTo' is required.");

        RuleFor(x => x.DateFrom)
            .LessThanOrEqualTo(x => x.DateTo)
            .WithMessage("'DateFrom' must be less than or equal to 'DateTo'.");

        RuleFor(x => x)
            .Must(x => (x.DateTo.DayNumber - x.DateFrom.DayNumber) <= 31)
            .WithMessage("Date range must not exceed 31 days.")
            .When(x => x.DateFrom <= x.DateTo);
    }
}
