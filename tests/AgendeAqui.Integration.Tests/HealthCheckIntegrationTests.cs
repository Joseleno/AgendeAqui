using AgendeAqui.Integration.Tests.Fixtures;
using FluentAssertions;
using System.Net;

namespace AgendeAqui.Integration.Tests;

public sealed class HealthCheckIntegrationTests : IntegrationTestBase
{
    public HealthCheckIntegrationTests(AgendeAquiWebAppFactory factory)
        : base(factory) { }

    [Fact]
    public async Task Health_ShouldReturnHealthy()
    {
        var response = await Client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthReady_ShouldReturnHealthy()
    {
        var response = await Client.GetAsync("/health/ready");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
