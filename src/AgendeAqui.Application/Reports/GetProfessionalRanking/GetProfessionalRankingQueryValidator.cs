using FluentValidation;

namespace AgendeAqui.Application.Reports.GetProfessionalRanking;

public sealed class GetProfessionalRankingQueryValidator : AbstractValidator<GetProfessionalRankingQuery>
{
    public GetProfessionalRankingQueryValidator()
    {
        RuleFor(x => x.From)
            .NotEmpty();

        RuleFor(x => x.To)
            .NotEmpty()
            .GreaterThanOrEqualTo(x => x.From)
            .WithMessage("'To' must be greater than or equal to 'From'.")
            .Must((query, to) => to.DayNumber - query.From.DayNumber <= 366)
            .WithMessage("Date range must not exceed 366 days.");
    }
}
