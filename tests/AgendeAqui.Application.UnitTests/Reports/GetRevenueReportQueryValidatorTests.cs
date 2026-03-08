using AgendeAqui.Application.Reports.GetRevenueReport;
using FluentValidation.TestHelper;

namespace AgendeAqui.Application.UnitTests.Reports;

public class GetRevenueReportQueryValidatorTests
{
    private readonly GetRevenueReportQueryValidator _validator = new();

    [Fact]
    public void Validate_WithValidDateRange_ShouldHaveNoErrors()
    {
        var query = new GetRevenueReportQuery(
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 1, 31));

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithSameDates_ShouldHaveNoErrors()
    {
        var date = new DateOnly(2026, 1, 15);
        var query = new GetRevenueReportQuery(date, date);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithToBeforeFrom_ShouldHaveError()
    {
        var query = new GetRevenueReportQuery(
            new DateOnly(2026, 1, 31),
            new DateOnly(2026, 1, 1));

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.To);
    }

    [Fact]
    public void Validate_WithDefaultFrom_ShouldHaveError()
    {
        var query = new GetRevenueReportQuery(default, new DateOnly(2026, 1, 31));

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.From);
    }

    [Fact]
    public void Validate_WithDefaultTo_ShouldHaveError()
    {
        var query = new GetRevenueReportQuery(new DateOnly(2026, 1, 1), default);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.To);
    }

    [Fact]
    public void Validate_WithRangeExceeding366Days_ShouldHaveError()
    {
        var query = new GetRevenueReportQuery(
            new DateOnly(2024, 1, 1),
            new DateOnly(2026, 1, 1));

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.To);
    }

    [Fact]
    public void Validate_With366DayRange_ShouldHaveNoErrors()
    {
        var query = new GetRevenueReportQuery(
            new DateOnly(2026, 1, 1),
            new DateOnly(2027, 1, 2));

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
