using FluentValidation;

namespace AgendeAqui.Application.Reports.GetAppointmentsTimeline;

public sealed class GetAppointmentsTimelineQueryValidator : AbstractValidator<GetAppointmentsTimelineQuery>
{
    public GetAppointmentsTimelineQueryValidator()
    {
        RuleFor(x => x.From)
            .LessThanOrEqualTo(x => x.To)
            .WithMessage("'From' must be less than or equal to 'To'.");

        RuleFor(x => x.GroupBy)
            .Must(g => g is "day" or "week" or "month")
            .WithMessage("'GroupBy' must be 'day', 'week', or 'month'.");
    }
}
