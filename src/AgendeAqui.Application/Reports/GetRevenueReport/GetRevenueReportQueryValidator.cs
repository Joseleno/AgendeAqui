using FluentValidation;

namespace AgendeAqui.Application.Reports.GetRevenueReport;

public sealed class GetRevenueReportQueryValidator : AbstractValidator<GetRevenueReportQuery>
{
    public GetRevenueReportQueryValidator()
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
