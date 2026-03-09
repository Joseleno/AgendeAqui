using AgendeAqui.Integration.Tests.Fixtures;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace AgendeAqui.Integration.Tests.Tenants;

public sealed class CrossTenantIsolationTests : IntegrationTestBase
{
    public CrossTenantIsolationTests(AgendeAquiWebAppFactory factory)
        : base(factory) { }

    [Fact]
    public async Task TenantA_CannotAccess_TenantB_Professional()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var phoneSuffix = Random.Shared.Next(10000000, 99999999).ToString();

        // Create Tenant A and a professional
        var tenantAId = await CreateTenantAsync($"Tenant A {suffix}", $"tenant-a-{suffix}");
        SetJwtToken(tenantAId, "Admin");

        var profResponse = await Client.PostAsJsonAsync("/api/v1/professionals",
            new { name = "Professional A", email = $"prof-a-{suffix}@test.com", phone = $"55119{phoneSuffix}" });
        profResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            await profResponse.Content.ReadAsStringAsync());
        var professionalId = (await profResponse.Content.ReadFromJsonAsync<IdResponse>())!.Id;

        // Create Tenant B
        var tenantBId = await CreateTenantAsync($"Tenant B {suffix}", $"tenant-b-{suffix}");

        // Switch to Tenant B context
        SetJwtToken(tenantBId, "Admin");

        // Tenant B should NOT be able to access Tenant A's professional
        var getResponse = await Client.GetAsync($"/api/v1/professionals/{professionalId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "RLS should prevent Tenant B from seeing Tenant A's data");
    }

    [Fact]
    public async Task TenantA_CannotAccess_TenantB_Client()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var phoneSuffix = Random.Shared.Next(10000000, 99999999).ToString();

        // Create Tenant A and a client
        var tenantAId = await CreateTenantAsync($"Tenant A {suffix}", $"tenant-a-cl-{suffix}");
        SetJwtToken(tenantAId, "Admin");

        var clientResponse = await Client.PostAsJsonAsync("/api/v1/clients",
            new { name = "Client A", email = $"client-a-{suffix}@test.com", phone = $"55119{phoneSuffix}" });
        clientResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            await clientResponse.Content.ReadAsStringAsync());
        var clientId = (await clientResponse.Content.ReadFromJsonAsync<IdResponse>())!.Id;

        // Create Tenant B and try to access Tenant A's client
        var tenantBId = await CreateTenantAsync($"Tenant B {suffix}", $"tenant-b-cl-{suffix}");
        SetJwtToken(tenantBId, "Admin");

        var getResponse = await Client.GetAsync($"/api/v1/clients/{clientId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "RLS should prevent Tenant B from seeing Tenant A's data");
    }

    private sealed record IdResponse(Guid Id);
}
