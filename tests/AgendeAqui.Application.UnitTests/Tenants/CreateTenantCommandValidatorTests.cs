using AgendeAqui.Application.Tenants.CreateTenant;
using FluentValidation.TestHelper;

namespace AgendeAqui.Application.UnitTests.Tenants;

public class CreateTenantCommandValidatorTests
{
    private readonly CreateTenantCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldHaveNoErrors()
    {
        var command = new CreateTenantCommand("Test", "test-tenant", "Free");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldHaveError()
    {
        var command = new CreateTenantCommand("", "test-tenant", "Free");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithNameExceedingMaxLength_ShouldHaveError()
    {
        var command = new CreateTenantCommand(new string('a', 201), "test-tenant", "Free");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithEmptySlug_ShouldHaveError()
    {
        var command = new CreateTenantCommand("Test", "", "Free");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }

    [Fact]
    public void Validate_WithSlugExceedingMaxLength_ShouldHaveError()
    {
        var command = new CreateTenantCommand("Test", new string('a', 101), "Free");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }

    [Fact]
    public void Validate_WithUppercaseSlug_ShouldHaveError()
    {
        var command = new CreateTenantCommand("Test", "Test-Tenant", "Free");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }

    [Fact]
    public void Validate_WithSlugContainingSpecialChars_ShouldHaveError()
    {
        var command = new CreateTenantCommand("Test", "test_tenant", "Free");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }

    [Fact]
    public void Validate_WithSlugStartingWithHyphen_ShouldHaveError()
    {
        var command = new CreateTenantCommand("Test", "-test", "Free");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }

    [Fact]
    public void Validate_WithSlugEndingWithHyphen_ShouldHaveError()
    {
        var command = new CreateTenantCommand("Test", "test-", "Free");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }

    [Fact]
    public void Validate_WithSlugHavingConsecutiveHyphens_ShouldHaveError()
    {
        var command = new CreateTenantCommand("Test", "test--tenant", "Free");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }

    [Fact]
    public void Validate_WithEmptyPlan_ShouldHaveError()
    {
        var command = new CreateTenantCommand("Test", "test-tenant", "");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Plan);
    }

    [Fact]
    public void Validate_WithInvalidPlan_ShouldHaveError()
    {
        var command = new CreateTenantCommand("Test", "test-tenant", "InvalidPlan");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Plan);
    }

    [Theory]
    [InlineData("Free")]
    [InlineData("Starter")]
    [InlineData("Professional")]
    [InlineData("Enterprise")]
    public void Validate_WithValidPlans_ShouldHaveNoErrors(string plan)
    {
        var command = new CreateTenantCommand("Test", "test-tenant", plan);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
