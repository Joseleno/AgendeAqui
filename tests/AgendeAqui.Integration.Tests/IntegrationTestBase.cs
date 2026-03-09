using AgendeAqui.Integration.Tests.Fixtures;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;

namespace AgendeAqui.Integration.Tests;

[Collection("Integration")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    private const string TestSecretKey = "integration-test-secret-key-must-be-at-least-32-chars";
    private const string TestIssuer = "AgendeAqui.Test";
    private const string TestAudience = "AgendeAqui.Api.Test";

    protected readonly AgendeAquiWebAppFactory Factory;
    protected HttpClient Client { get; private set; } = default!;

    protected IntegrationTestBase(AgendeAquiWebAppFactory factory)
    {
        Factory = factory;
    }

    public ValueTask InitializeAsync()
    {
        Client = Factory.CreateClient();
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        Client.Dispose();
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Adds X-Tenant-Id header to all subsequent requests via this client.
    /// </summary>
    protected void SetTenantHeader(Guid tenantId)
    {
        Client.DefaultRequestHeaders.Remove("X-Tenant-Id");
        Client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());
    }

    /// <summary>
    /// Sets the Authorization header with a JWT token for the given role and tenant.
    /// Roles: Admin, Professional, Client, ApiKey
    /// </summary>
    protected void SetJwtToken(Guid tenantId, string role = "Admin", Guid? userId = null, int plan = 1)
    {
        var token = GenerateJwtToken(userId ?? Guid.NewGuid(), tenantId, role, plan);
        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        SetTenantHeader(tenantId);
    }

    private static string GenerateJwtToken(Guid userId, Guid tenantId, string role, int plan = 1)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("tenant_id", tenantId.ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim("role", role),
            new Claim("tenant_plan", plan.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: TestIssuer,
            audience: TestAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Creates a tenant via the API and returns its Id.
    /// </summary>
    protected async Task<Guid> CreateTenantAsync(string name = "Test Tenant", string slug = "test-tenant", string plan = "Free")
    {
        // Tenant creation uses RequireAdmin policy, set a temporary admin token
        var tempAdminToken = GenerateJwtToken(Guid.NewGuid(), Guid.Empty, "Admin");
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/tenants");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tempAdminToken);
        request.Content = JsonContent.Create(new { name, slug, plan });

        var response = await Client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<IdResponse>();
        return result!.Id;
    }

    private sealed record IdResponse(Guid Id);
}
