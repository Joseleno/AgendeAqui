using AgendeAqui.Application.Tenants.UpdateTenant;
using FluentValidation.TestHelper;

namespace AgendeAqui.Application.UnitTests.Tenants;

public class UpdateTenantCommandValidatorTests
{
    private readonly UpdateTenantCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        var command = new UpdateTenantCommand(Guid.NewGuid(), "Valid Name", "Professional");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyTenantId_ShouldHaveError()
    {
        var command = new UpdateTenantCommand(Guid.Empty, "Name", "Free");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldHaveError()
    {
        var command = new UpdateTenantCommand(Guid.NewGuid(), "", "Free");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithNameTooLong_ShouldHaveError()
    {
        var command = new UpdateTenantCommand(Guid.NewGuid(), new string('a', 201), "Free");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithInvalidPlan_ShouldHaveError()
    {
        var command = new UpdateTenantCommand(Guid.NewGuid(), "Name", "InvalidPlan");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Plan);
    }

    [Fact]
    public void Validate_WithEmptyPlan_ShouldHaveError()
    {
        var command = new UpdateTenantCommand(Guid.NewGuid(), "Name", "");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Plan);
    }

    [Theory]
    [InlineData("Free")]
    [InlineData("Starter")]
    [InlineData("Professional")]
    [InlineData("Enterprise")]
    public void Validate_WithValidPlans_ShouldNotHaveErrors(string plan)
    {
        var command = new UpdateTenantCommand(Guid.NewGuid(), "Valid Name", plan);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithWhitespaceName_ShouldHaveError()
    {
        var command = new UpdateTenantCommand(Guid.NewGuid(), "   ", "Free");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}
