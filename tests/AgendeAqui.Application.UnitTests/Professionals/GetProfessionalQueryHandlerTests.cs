using AgendeAqui.Application.Professionals.GetProfessional;
using FluentAssertions;

namespace AgendeAqui.Application.UnitTests.Professionals;

// Note: GetProfessionalQueryHandler.Handle() uses Dapper with DbConnection which cannot be
// unit-tested without a real database. Handler logic is covered by integration tests.
// These tests verify the query record and response DTO contracts.
public class GetProfessionalQueryHandlerTests
{
    [Fact]
    public void GetProfessionalQuery_ShouldStoreProfessionalId()
    {
        var professionalId = Guid.NewGuid();
        var query = new GetProfessionalQuery(professionalId);

        query.ProfessionalId.Should().Be(professionalId);
    }

    [Fact]
    public void ProfessionalResponse_ShouldHaveExpectedProperties()
    {
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var response = new ProfessionalResponse(
            id, "Maria Silva", "maria@example.com", "+5511999990000",
            true, createdAt);

        response.Id.Should().Be(id);
        response.Name.Should().Be("Maria Silva");
        response.Email.Should().Be("maria@example.com");
        response.Phone.Should().Be("+5511999990000");
        response.IsActive.Should().BeTrue();
        response.CreatedAt.Should().Be(createdAt);
    }
}
