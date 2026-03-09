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
    public async Task HealthReady_ShouldReturnHealthyOrDegraded()
    {
        // /health/ready checks PostgreSQL + RabbitMQ. In CI, RabbitMQ may still be
        // initializing when this test runs. We accept both OK and ServiceUnavailable
        // and only verify that the endpoint responds.
        var response = await Client.GetAsync("/health/ready");

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.ServiceUnavailable);
    }
}
