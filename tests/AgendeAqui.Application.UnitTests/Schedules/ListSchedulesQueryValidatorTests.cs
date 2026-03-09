using AgendeAqui.Application.Schedules.ListSchedules;
using FluentValidation.TestHelper;

namespace AgendeAqui.Application.UnitTests.Schedules;

public class ListSchedulesQueryValidatorTests
{
    private readonly ListSchedulesQueryValidator _validator = new();

    [Fact]
    public void Validate_WithValidQuery_ShouldHaveNoErrors()
    {
        var query = new ListSchedulesQuery(1, 10);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithZeroPage_ShouldHaveError()
    {
        var query = new ListSchedulesQuery(0, 10);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Fact]
    public void Validate_WithZeroPageSize_ShouldHaveError()
    {
        var query = new ListSchedulesQuery(1, 0);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Validate_WithPageSizeAtMax_ShouldHaveNoErrors()
    {
        // PagedRequest clamps values > 50 to 50, so 51 becomes 50 and passes validation.
        // This test verifies the boundary at exactly MaxPageSize (50) is valid.
        var query = new ListSchedulesQuery(1, 50);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Validate_WithNegativePage_ShouldHaveError()
    {
        var query = new ListSchedulesQuery(-1, 10);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Page);
    }
}
