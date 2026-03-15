using FluentValidation;

namespace AgendeAqui.Application.Reports.GetRevenueTimeline;

public sealed class GetRevenueTimelineQueryValidator : AbstractValidator<GetRevenueTimelineQuery>
{
    public GetRevenueTimelineQueryValidator()
    {
        RuleFor(x => x.From)
            .LessThanOrEqualTo(x => x.To)
            .WithMessage("'From' must be less than or equal to 'To'.");
    }
}
