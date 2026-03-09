using AgendeAqui.Application.Tenants.GetTenant;
using FluentAssertions;

namespace AgendeAqui.Application.UnitTests.Tenants;

// Note: GetTenantQueryHandler.Handle() uses Dapper with DbConnection which cannot be
// unit-tested without a real database. Handler logic is covered by integration tests.
// These tests verify the query record and response DTO contracts.
public class GetTenantQueryHandlerTests
{
    [Fact]
    public void GetTenantQuery_ShouldStoreTenantId()
    {
        var tenantId = Guid.NewGuid();
        var query = new GetTenantQuery(tenantId);

        query.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public void TenantResponse_ShouldHaveExpectedProperties()
    {
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var response = new TenantResponse(
            id, "Barbearia do Joao", "barbearia-do-joao",
            "Active", "Professional", createdAt);

        response.Id.Should().Be(id);
        response.Name.Should().Be("Barbearia do Joao");
        response.Slug.Should().Be("barbearia-do-joao");
        response.Status.Should().Be("Active");
        response.Plan.Should().Be("Professional");
        response.CreatedAt.Should().Be(createdAt);
    }
}
