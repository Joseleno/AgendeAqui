using AgendeAqui.Application.Clients.GetClient;
using FluentAssertions;

namespace AgendeAqui.Application.UnitTests.Clients;

// Note: GetClientQueryHandler.Handle() uses Dapper with DbConnection which cannot be
// unit-tested without a real database. Handler logic is covered by integration tests.
// These tests verify the query record and response DTO contracts.
public class GetClientQueryHandlerTests
{
    [Fact]
    public void GetClientQuery_ShouldStoreClientId()
    {
        var clientId = Guid.NewGuid();
        var query = new GetClientQuery(clientId);

        query.ClientId.Should().Be(clientId);
    }

    [Fact]
    public void ClientResponse_ShouldHaveExpectedProperties()
    {
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var response = new ClientResponse(
            id, "Joao Santos", "joao@example.com", "+5511988880000",
            createdAt);

        response.Id.Should().Be(id);
        response.Name.Should().Be("Joao Santos");
        response.Email.Should().Be("joao@example.com");
        response.Phone.Should().Be("+5511988880000");
        response.CreatedAt.Should().Be(createdAt);
    }
}
