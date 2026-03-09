using AgendeAqui.Application.Tenants.ListTenants;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace AgendeAqui.Application.UnitTests.Tenants;

public class ListTenantsQueryValidatorTests
{
    private readonly ListTenantsQueryValidator _validator = new();

    [Fact]
    public void Validate_WithDefaults_ShouldNotHaveErrors()
    {
        var query = new ListTenantsQuery();
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithValidStatus_ShouldNotHaveErrors()
    {
        var query = new ListTenantsQuery { Status = "Active" };
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithInvalidStatus_ShouldHaveError()
    {
        var query = new ListTenantsQuery { Status = "InvalidStatus" };
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Validate_WithPageLessThanOne_ShouldHaveError()
    {
        var query = new ListTenantsQuery { Page = 0 };
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Fact]
    public void Validate_WithPageSizeTooLarge_ShouldBeClampedByPagedRequest()
    {
        // PagedRequest clamps PageSize to 50, so 51 becomes 50 and passes validation
        var query = new ListTenantsQuery { PageSize = 51 };
        query.PageSize.Should().Be(50);
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithPageSizeZero_ShouldHaveError()
    {
        var query = new ListTenantsQuery { PageSize = 0 };
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Theory]
    [InlineData("Active")]
    [InlineData("Inactive")]
    [InlineData("Suspended")]
    public void Validate_WithAllValidStatuses_ShouldNotHaveErrors(string status)
    {
        var query = new ListTenantsQuery { Status = status };
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
