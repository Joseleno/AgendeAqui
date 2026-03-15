using FluentValidation;

namespace AgendeAqui.Application.Reports.GetBusiestHours;

public sealed class GetBusiestHoursQueryValidator : AbstractValidator<GetBusiestHoursQuery>
{
    public GetBusiestHoursQueryValidator()
    {
        RuleFor(x => x.From)
            .LessThanOrEqualTo(x => x.To)
            .WithMessage("'From' must be less than or equal to 'To'.");
    }
}
