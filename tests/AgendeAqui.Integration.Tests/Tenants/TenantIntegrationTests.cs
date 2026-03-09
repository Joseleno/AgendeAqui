using AgendeAqui.Integration.Tests.Fixtures;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace AgendeAqui.Integration.Tests.Tenants;

public sealed class TenantIntegrationTests : IntegrationTestBase
{
    public TenantIntegrationTests(AgendeAquiWebAppFactory factory)
        : base(factory) { }

    [Fact]
    public async Task CreateTenant_WithValidData_ShouldReturn201()
    {
        SetAdminToken();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var request = new { name = "Barbearia do João", slug = $"barbearia-joao-{suffix}", plan = "Free" };

        var response = await Client.PostAsJsonAsync("/api/v1/tenants", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<IdResponse>();
        body.Should().NotBeNull();
        body!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateTenant_WithEmptyName_ShouldReturn400()
    {
        SetAdminToken();
        var request = new { name = "", slug = "some-slug", plan = "Free" };

        var response = await Client.PostAsJsonAsync("/api/v1/tenants", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTenant_WithValidId_ShouldReturn200()
    {
        SetAdminToken();
        var tenantId = await CreateTenantAsync("Clinica ABC", "clinica-abc-" + Guid.NewGuid().ToString("N")[..8]);
        SetAdminToken();

        var response = await Client.GetAsync($"/api/v1/tenants/{tenantId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<TenantResponse>();
        body.Should().NotBeNull();
        body!.Id.Should().Be(tenantId);
        body.Name.Should().Be("Clinica ABC");
    }

    [Fact]
    public async Task GetTenant_WithNonExistentId_ShouldReturn404()
    {
        SetAdminToken();

        var response = await Client.GetAsync($"/api/v1/tenants/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateTenant_WithoutAuth_ShouldReturnError()
    {
        // Without auth, TenantResolutionMiddleware returns 400 for missing X-Tenant-Id
        // before the authorization layer kicks in (middleware runs before auth filters).
        var request = new { name = "Unauthorized Tenant", slug = "unauth", plan = "Free" };

        using var noAuthClient = Factory.CreateClient();
        var response = await noAuthClient.PostAsJsonAsync("/api/v1/tenants", request);

        // Middleware ordering: TenantResolution runs before Authorization,
        // so unauthenticated requests without X-Tenant-Id get 400 (not 401).
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private void SetAdminToken()
    {
        SetJwtToken(Guid.Empty, "Admin");
    }

    private sealed record IdResponse(Guid Id);
    private sealed record TenantResponse(Guid Id, string Name, string Slug, string Status, string Plan, DateTime CreatedAt);
}
