using AgendeAqui.Application.Services.GetService;
using FluentAssertions;

namespace AgendeAqui.Application.UnitTests.Services;

// Note: GetServiceQueryHandler.Handle() uses Dapper with DbConnection which cannot be
// unit-tested without a real database. Handler logic is covered by integration tests.
// These tests verify the query record and response DTO contracts.
public class GetServiceQueryHandlerTests
{
    [Fact]
    public void GetServiceQuery_ShouldStoreServiceId()
    {
        var serviceId = Guid.NewGuid();
        var query = new GetServiceQuery(serviceId);

        query.ServiceId.Should().Be(serviceId);
    }

    [Fact]
    public void ServiceResponse_ShouldHaveExpectedProperties()
    {
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var response = new ServiceResponse(
            id, "Corte de Cabelo", 30, 50.00m,
            true, createdAt);

        response.Id.Should().Be(id);
        response.Name.Should().Be("Corte de Cabelo");
        response.DurationMinutes.Should().Be(30);
        response.Price.Should().Be(50.00m);
        response.IsActive.Should().BeTrue();
        response.CreatedAt.Should().Be(createdAt);
    }
}
