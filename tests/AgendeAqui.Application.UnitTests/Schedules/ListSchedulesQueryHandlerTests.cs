using AgendeAqui.Application.Schedules.ListSchedules;
using FluentAssertions;

namespace AgendeAqui.Application.UnitTests.Schedules;

// Note: ListSchedulesQueryHandler.Handle() uses Dapper with DbConnection which cannot be
// unit-tested without a real database. Handler logic is covered by integration tests.
// These tests verify the query record contracts.
public class ListSchedulesQueryHandlerTests
{
    [Fact]
    public void ListSchedulesQuery_ShouldSetDefaultValues()
    {
        var query = new ListSchedulesQuery(1, 10);

        query.Page.Should().Be(1);
        query.PageSize.Should().Be(10);
        query.ProfessionalId.Should().BeNull();
    }

    [Fact]
    public void ListSchedulesQuery_WithProfessionalId_ShouldSetFilter()
    {
        var professionalId = Guid.NewGuid();
        var query = new ListSchedulesQuery(2, 20, professionalId);

        query.Page.Should().Be(2);
        query.PageSize.Should().Be(20);
        query.ProfessionalId.Should().Be(professionalId);
    }
}
