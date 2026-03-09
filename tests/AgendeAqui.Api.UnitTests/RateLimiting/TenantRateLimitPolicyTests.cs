using AgendeAqui.Api.RateLimiting;
using FluentAssertions;

namespace AgendeAqui.Api.UnitTests.RateLimiting;

public class TenantRateLimitPolicyTests
{
    [Fact]
    public void GetPermitLimit_FreePlan_Returns30()
    {
        var limit = TenantRateLimitPolicy.GetPermitLimit("1");

        limit.Should().Be(30);
    }

    [Fact]
    public void GetPermitLimit_StarterPlan_Returns100()
    {
        var limit = TenantRateLimitPolicy.GetPermitLimit("2");

        limit.Should().Be(100);
    }

    [Fact]
    public void GetPermitLimit_ProfessionalPlan_Returns1000()
    {
        var limit = TenantRateLimitPolicy.GetPermitLimit("3");

        limit.Should().Be(1000);
    }

    [Fact]
    public void GetPermitLimit_EnterprisePlan_Returns5000()
    {
        var limit = TenantRateLimitPolicy.GetPermitLimit("4");

        limit.Should().Be(5000);
    }

    [Fact]
    public void GetPermitLimit_NullClaim_ReturnsFreePlanDefault()
    {
        var limit = TenantRateLimitPolicy.GetPermitLimit(null);

        limit.Should().Be(30);
    }

    [Fact]
    public void GetPermitLimit_EmptyClaim_ReturnsFreePlanDefault()
    {
        var limit = TenantRateLimitPolicy.GetPermitLimit(string.Empty);

        limit.Should().Be(30);
    }

    [Fact]
    public void GetPermitLimit_UnknownPlanValue_ReturnsFreePlanDefault()
    {
        var limit = TenantRateLimitPolicy.GetPermitLimit("99");

        limit.Should().Be(30);
    }

    [Fact]
    public void GetPermitLimit_NonNumericClaim_ReturnsFreePlanDefault()
    {
        var limit = TenantRateLimitPolicy.GetPermitLimit("unknown");

        limit.Should().Be(30);
    }
}
