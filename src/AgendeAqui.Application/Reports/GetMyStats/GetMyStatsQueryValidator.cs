using FluentValidation;

namespace AgendeAqui.Application.Reports.GetMyStats;

public sealed class GetMyStatsQueryValidator : AbstractValidator<GetMyStatsQuery>
{
    public GetMyStatsQueryValidator()
    {
        RuleFor(x => x.From)
            .NotEmpty();

        RuleFor(x => x.To)
            .NotEmpty()
            .GreaterThanOrEqualTo(x => x.From)
            .WithMessage("'To' must be greater than or equal to 'From'.");
    }
}
