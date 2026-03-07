using AgendeAqui.Api.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Security.Claims;

namespace AgendeAqui.Api.UnitTests.Auth;

public class CurrentUserServiceTests
{
    private static CurrentUserService CreateService(params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = principal };
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(httpContext);

        return new CurrentUserService(accessor);
    }

    [Fact]
    public void UserId_WithNameIdentifierClaim_ShouldReturnGuid()
    {
        var userId = Guid.NewGuid();
        var service = CreateService(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));

        service.UserId.Should().Be(userId);
    }

    [Fact]
    public void UserId_WithSubClaim_ShouldReturnGuid()
    {
        var userId = Guid.NewGuid();
        var service = CreateService(new Claim("sub", userId.ToString()));

        service.UserId.Should().Be(userId);
    }

    [Fact]
    public void UserId_WithNoClaims_ShouldReturnEmptyGuid()
    {
        var service = CreateService();

        service.UserId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void TenantId_WithTenantIdClaim_ShouldReturnGuid()
    {
        var tenantId = Guid.NewGuid();
        var service = CreateService(new Claim("tenant_id", tenantId.ToString()));

        service.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public void TenantId_WithNoClaims_ShouldReturnEmptyGuid()
    {
        var service = CreateService();

        service.TenantId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void Role_WithRoleClaim_ShouldReturnRole()
    {
        var service = CreateService(new Claim(ClaimTypes.Role, "Admin"));

        service.Role.Should().Be("Admin");
    }

    [Fact]
    public void Role_WithCustomRoleClaim_ShouldReturnRole()
    {
        var service = CreateService(new Claim("role", "Professional"));

        service.Role.Should().Be("Professional");
    }

    [Fact]
    public void Role_WithNoClaims_ShouldReturnEmpty()
    {
        var service = CreateService();

        service.Role.Should().BeEmpty();
    }

    [Fact]
    public void IsAdmin_WithAdminRole_ShouldReturnTrue()
    {
        var service = CreateService(new Claim(ClaimTypes.Role, "Admin"));

        service.IsAdmin.Should().BeTrue();
    }

    [Fact]
    public void IsAdmin_WithClientRole_ShouldReturnFalse()
    {
        var service = CreateService(new Claim(ClaimTypes.Role, "Client"));

        service.IsAdmin.Should().BeFalse();
    }

    [Fact]
    public void IsProfessional_WithProfessionalRole_ShouldReturnTrue()
    {
        var service = CreateService(new Claim(ClaimTypes.Role, "Professional"));

        service.IsProfessional.Should().BeTrue();
    }

    [Fact]
    public void IsProfessional_WithAdminRole_ShouldReturnTrue()
    {
        var service = CreateService(new Claim(ClaimTypes.Role, "Admin"));

        service.IsProfessional.Should().BeTrue();
    }

    [Fact]
    public void IsProfessional_WithClientRole_ShouldReturnFalse()
    {
        var service = CreateService(new Claim(ClaimTypes.Role, "Client"));

        service.IsProfessional.Should().BeFalse();
    }

    [Fact]
    public void IsClient_WithClientRole_ShouldReturnTrue()
    {
        var service = CreateService(new Claim(ClaimTypes.Role, "Client"));

        service.IsClient.Should().BeTrue();
    }

    [Fact]
    public void IsClient_WithAdminRole_ShouldReturnTrue()
    {
        var service = CreateService(new Claim(ClaimTypes.Role, "Admin"));

        service.IsClient.Should().BeTrue();
    }

    [Fact]
    public void IsClient_WithProfessionalRole_ShouldReturnTrue()
    {
        var service = CreateService(new Claim(ClaimTypes.Role, "Professional"));

        service.IsClient.Should().BeTrue();
    }

    [Fact]
    public void WithNoHttpContext_ShouldReturnDefaults()
    {
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns((HttpContext?)null);

        var service = new CurrentUserService(accessor);

        service.UserId.Should().Be(Guid.Empty);
        service.TenantId.Should().Be(Guid.Empty);
        service.Role.Should().BeEmpty();
        service.IsAdmin.Should().BeFalse();
    }
}
