using AgendeAqui.Api.Middleware;
using AgendeAqui.Domain.Abstractions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Security.Claims;

namespace AgendeAqui.Api.UnitTests.Middleware;

public class TenantResolutionMiddlewareTests
{
    private readonly ITenantProvider _tenantProvider;
    private bool _nextCalled;

    public TenantResolutionMiddlewareTests()
    {
        _tenantProvider = Substitute.For<ITenantProvider>();
        _nextCalled = false;
    }

    private TenantResolutionMiddleware CreateMiddleware() =>
        new(context =>
        {
            _nextCalled = true;
            return Task.CompletedTask;
        });

    private DefaultHttpContext CreateContext(string path = "/api/v1/tenants") =>
        new() { Request = { Path = path } };

    [Fact]
    public async Task InvokeAsync_WithExcludedHealthPath_ShouldSkipResolution()
    {
        var middleware = CreateMiddleware();
        var context = CreateContext("/health");

        await middleware.InvokeAsync(context, _tenantProvider);

        _nextCalled.Should().BeTrue();
        _tenantProvider.DidNotReceive().SetTenantId(Arg.Any<Guid>());
    }

    [Fact]
    public async Task InvokeAsync_WithExcludedMetricsPath_ShouldSkipResolution()
    {
        var middleware = CreateMiddleware();
        var context = CreateContext("/metrics");

        await middleware.InvokeAsync(context, _tenantProvider);

        _nextCalled.Should().BeTrue();
        _tenantProvider.DidNotReceive().SetTenantId(Arg.Any<Guid>());
    }

    [Fact]
    public async Task InvokeAsync_WithExcludedOpenApiPath_ShouldSkipResolution()
    {
        var middleware = CreateMiddleware();
        var context = CreateContext("/openapi/v1.json");

        await middleware.InvokeAsync(context, _tenantProvider);

        _nextCalled.Should().BeTrue();
        _tenantProvider.DidNotReceive().SetTenantId(Arg.Any<Guid>());
    }

    [Fact]
    public async Task InvokeAsync_WithAuthenticatedUserAndTenantClaim_ShouldSetTenantFromClaim()
    {
        var middleware = CreateMiddleware();
        var tenantId = Guid.NewGuid();
        var context = CreateContext();

        var claims = new[] { new Claim("tenant_id", tenantId.ToString()) };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestScheme"));

        await middleware.InvokeAsync(context, _tenantProvider);

        _nextCalled.Should().BeTrue();
        _tenantProvider.Received(1).SetTenantId(tenantId);
    }

    [Fact]
    public async Task InvokeAsync_WithAuthenticatedUserWithoutTenantClaim_ShouldReturn403()
    {
        var middleware = CreateMiddleware();
        var context = CreateContext();

        context.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("sub", Guid.NewGuid().ToString())], "TestScheme"));

        await middleware.InvokeAsync(context, _tenantProvider);

        _nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        _tenantProvider.DidNotReceive().SetTenantId(Arg.Any<Guid>());
    }

    [Fact]
    public async Task InvokeAsync_WithAuthenticatedUserWithMalformedTenantClaim_ShouldReturn403()
    {
        var middleware = CreateMiddleware();
        var context = CreateContext();

        context.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("tenant_id", "not-a-guid")], "TestScheme"));

        await middleware.InvokeAsync(context, _tenantProvider);

        _nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        _tenantProvider.DidNotReceive().SetTenantId(Arg.Any<Guid>());
    }

    [Fact]
    public async Task InvokeAsync_WithValidTenantIdHeader_ShouldSetTenantFromHeader()
    {
        var middleware = CreateMiddleware();
        var tenantId = Guid.NewGuid();
        var context = CreateContext();
        context.Request.Headers["X-Tenant-Id"] = tenantId.ToString();

        await middleware.InvokeAsync(context, _tenantProvider);

        _nextCalled.Should().BeTrue();
        _tenantProvider.Received(1).SetTenantId(tenantId);
    }

    [Fact]
    public async Task InvokeAsync_WithMissingTenantIdHeader_ShouldReturn400()
    {
        var middleware = CreateMiddleware();
        var context = CreateContext();

        await middleware.InvokeAsync(context, _tenantProvider);

        _nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        _tenantProvider.DidNotReceive().SetTenantId(Arg.Any<Guid>());
    }

    [Fact]
    public async Task InvokeAsync_WithInvalidTenantIdHeader_ShouldReturn400()
    {
        var middleware = CreateMiddleware();
        var context = CreateContext();
        context.Request.Headers["X-Tenant-Id"] = "not-a-guid";

        await middleware.InvokeAsync(context, _tenantProvider);

        _nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        _tenantProvider.DidNotReceive().SetTenantId(Arg.Any<Guid>());
    }

    [Fact]
    public async Task InvokeAsync_WithWhitespaceTenantIdHeader_ShouldReturn400()
    {
        var middleware = CreateMiddleware();
        var context = CreateContext();
        context.Request.Headers["X-Tenant-Id"] = "   ";

        await middleware.InvokeAsync(context, _tenantProvider);

        _nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        _tenantProvider.DidNotReceive().SetTenantId(Arg.Any<Guid>());
    }
}
