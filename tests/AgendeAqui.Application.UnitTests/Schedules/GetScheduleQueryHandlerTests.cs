using AgendeAqui.Application.Schedules.GetSchedule;
using FluentAssertions;

namespace AgendeAqui.Application.UnitTests.Schedules;

// Note: GetScheduleQueryHandler.Handle() uses Dapper with DbConnection which cannot be
// unit-tested without a real database. Handler logic is covered by integration tests.
// These tests verify the query record and response DTO contracts.
public class GetScheduleQueryHandlerTests
{
    [Fact]
    public void GetScheduleQuery_ShouldStoreScheduleId()
    {
        var scheduleId = Guid.NewGuid();
        var query = new GetScheduleQuery(scheduleId);

        query.ScheduleId.Should().Be(scheduleId);
    }

    [Fact]
    public void ScheduleResponse_ShouldHaveExpectedProperties()
    {
        var id = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var response = new ScheduleResponse(
            id, professionalId, "John", 1,
            new TimeOnly(8, 0), new TimeOnly(18, 0),
            30, true, createdAt);

        response.Id.Should().Be(id);
        response.ProfessionalId.Should().Be(professionalId);
        response.ProfessionalName.Should().Be("John");
        response.DayOfWeek.Should().Be(1);
        response.StartTime.Should().Be(new TimeOnly(8, 0));
        response.EndTime.Should().Be(new TimeOnly(18, 0));
        response.SlotDurationMinutes.Should().Be(30);
        response.IsActive.Should().BeTrue();
        response.CreatedAt.Should().Be(createdAt);
    }
}
