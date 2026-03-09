using AgendeAqui.Application.Schedules.ListSchedules;
using FluentAssertions;

namespace AgendeAqui.Application.UnitTests.Common;

public class PagedRequestTests
{
    [Fact]
    public void PageSize_WhenExceedsMax_ShouldClampToMax()
    {
        var query = new ListSchedulesQuery(1, 101);

        query.PageSize.Should().Be(50);
    }

    [Fact]
    public void PageSize_WhenWithinMax_ShouldReturnAsIs()
    {
        var query = new ListSchedulesQuery(1, 25);

        query.PageSize.Should().Be(25);
    }

    [Fact]
    public void PageSize_WhenExactlyMax_ShouldReturnMax()
    {
        var query = new ListSchedulesQuery(1, 50);

        query.PageSize.Should().Be(50);
    }
}
