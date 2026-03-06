using AgendeAqui.Domain.Tenants;
using FluentAssertions;

namespace AgendeAqui.Domain.UnitTests.Tenants;

public class TenantTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnActiveTenant()
    {
        var tenant = Tenant.Create("Acme Corp", "acme-corp", TenantPlan.Starter);

        tenant.Name.Should().Be("Acme Corp");
        tenant.Slug.Should().Be("acme-corp");
        tenant.Plan.Should().Be(TenantPlan.Starter);
        tenant.Status.Should().Be(TenantStatus.Active);
    }

    [Fact]
    public void Create_ShouldNormalize_SlugToLowercase()
    {
        var tenant = Tenant.Create("Acme Corp", "Acme-Corp");

        tenant.Slug.Should().Be("acme-corp");
    }

    [Fact]
    public void Create_WithDefaultPlan_ShouldUseFree()
    {
        var tenant = Tenant.Create("Clinic X", "clinic-x");

        tenant.Plan.Should().Be(TenantPlan.Free);
    }

    [Fact]
    public void Create_WithNullName_ShouldThrowArgumentException()
    {
        var act = () => Tenant.Create(null!, "valid-slug");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowArgumentException()
    {
        var act = () => Tenant.Create("", "valid-slug");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithWhitespaceName_ShouldThrowArgumentException()
    {
        var act = () => Tenant.Create("   ", "valid-slug");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNullSlug_ShouldThrowArgumentException()
    {
        var act = () => Tenant.Create("Valid Name", null!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptySlug_ShouldThrowArgumentException()
    {
        var act = () => Tenant.Create("Valid Name", "");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_ShouldSetStatusToInactive()
    {
        var tenant = Tenant.Create("Clinic X", "clinic-x");

        tenant.Deactivate();

        tenant.Status.Should().Be(TenantStatus.Inactive);
        tenant.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Activate_AfterDeactivation_ShouldSetStatusToActive()
    {
        var tenant = Tenant.Create("Clinic X", "clinic-x");
        tenant.Deactivate();

        tenant.Activate();

        tenant.Status.Should().Be(TenantStatus.Active);
        tenant.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void ChangePlan_ShouldUpdatePlan()
    {
        var tenant = Tenant.Create("Clinic X", "clinic-x");

        tenant.ChangePlan(TenantPlan.Professional);

        tenant.Plan.Should().Be(TenantPlan.Professional);
        tenant.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void ChangePlan_ToEnterprise_ShouldUpdatePlan()
    {
        var tenant = Tenant.Create("Clinic X", "clinic-x", TenantPlan.Starter);

        tenant.ChangePlan(TenantPlan.Enterprise);

        tenant.Plan.Should().Be(TenantPlan.Enterprise);
    }
}
