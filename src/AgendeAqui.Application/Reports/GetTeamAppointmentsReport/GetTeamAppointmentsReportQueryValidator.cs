using FluentValidation;

namespace AgendeAqui.Application.Reports.GetTeamAppointmentsReport;

internal sealed class GetTeamAppointmentsReportQueryValidator : AbstractValidator<GetTeamAppointmentsReportQuery>
{
    private const int MaxDaysRange = 366;

    public GetTeamAppointmentsReportQueryValidator()
    {
        RuleFor(x => x.From).NotEmpty();
        RuleFor(x => x.To).NotEmpty().GreaterThanOrEqualTo(x => x.From);
        RuleFor(x => x)
            .Must(x => (x.To.ToDateTime(TimeOnly.MinValue) - x.From.ToDateTime(TimeOnly.MinValue)).TotalDays <= MaxDaysRange)
            .WithMessage($"Date range cannot exceed {MaxDaysRange} days.");
    }
}
