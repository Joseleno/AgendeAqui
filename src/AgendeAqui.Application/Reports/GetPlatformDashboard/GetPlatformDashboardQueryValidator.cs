using FluentValidation;

namespace AgendeAqui.Application.Reports.GetPlatformDashboard;

internal sealed class GetPlatformDashboardQueryValidator : AbstractValidator<GetPlatformDashboardQuery>
{
    public GetPlatformDashboardQueryValidator()
    {
        RuleFor(x => x.From).NotEmpty();
        RuleFor(x => x.To).NotEmpty().GreaterThanOrEqualTo(x => x.From);
        RuleFor(x => x)
            .Must(x => (x.To.ToDateTime(TimeOnly.MinValue) - x.From.ToDateTime(TimeOnly.MinValue)).TotalDays <= 366)
            .WithMessage("Date range cannot exceed 366 days.");
    }
}
