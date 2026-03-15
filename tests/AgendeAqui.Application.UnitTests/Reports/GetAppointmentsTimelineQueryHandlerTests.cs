using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Reports.GetAppointmentsTimeline;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using FluentAssertions;
using Microsoft.Extensions.Caching.Hybrid;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Reports;

public class GetAppointmentsTimelineQueryHandlerTests
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly HybridCache _cache;
    private readonly GetAppointmentsTimelineQueryHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public GetAppointmentsTimelineQueryHandlerTests()
    {
        _sqlConnectionFactory = Substitute.For<ISqlConnectionFactory>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _cache = Substitute.For<HybridCache>();
        _tenantProvider.GetTenantId().Returns(_tenantId);
        _handler = new GetAppointmentsTimelineQueryHandler(_sqlConnectionFactory, _tenantProvider, _cache);
    }

    [Fact]
    public async Task Handle_WhenCacheHit_ShouldReturnCachedResponse()
    {
        // Arrange
        var query = new GetAppointmentsTimelineQuery(
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 1, 31));
        var cachedResponse = new AppointmentsTimelineResponse(
            [new TimelinePoint("2026-01-15", 5, 3, 1, 0)]);

        _cache.GetOrCreateAsync<AppointmentsTimelineResponse>(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<AppointmentsTimelineResponse>>>(),
                Arg.Any<HybridCacheEntryOptions>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>())
            .Returns(cachedResponse);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Points.Should().HaveCount(1);
        result.Value.Points[0].Period.Should().Be("2026-01-15");
        result.Value.Points[0].Scheduled.Should().Be(5);
        result.Value.Points[0].Completed.Should().Be(3);
    }

    [Fact]
    public async Task Handle_WithDayGroupBy_ShouldUseDayCacheKey()
    {
        // Arrange
        var from = new DateOnly(2026, 3, 1);
        var to = new DateOnly(2026, 3, 31);
        var query = new GetAppointmentsTimelineQuery(from, to, "day");
        var expectedKey = $"appointments-timeline:{_tenantId}:{from}:{to}:day";
        var emptyResponse = new AppointmentsTimelineResponse([]);

        _cache.GetOrCreateAsync<AppointmentsTimelineResponse>(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<AppointmentsTimelineResponse>>>(),
                Arg.Any<HybridCacheEntryOptions>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>())
            .Returns(emptyResponse);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _cache.Received(1).GetOrCreateAsync<AppointmentsTimelineResponse>(
            expectedKey,
            Arg.Any<Func<CancellationToken, ValueTask<AppointmentsTimelineResponse>>>(),
            Arg.Any<HybridCacheEntryOptions>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithWeekGroupBy_ShouldUseWeekCacheKey()
    {
        // Arrange
        var from = new DateOnly(2026, 3, 1);
        var to = new DateOnly(2026, 3, 31);
        var query = new GetAppointmentsTimelineQuery(from, to, "week");
        var expectedKey = $"appointments-timeline:{_tenantId}:{from}:{to}:week";
        var emptyResponse = new AppointmentsTimelineResponse([]);

        _cache.GetOrCreateAsync<AppointmentsTimelineResponse>(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<AppointmentsTimelineResponse>>>(),
                Arg.Any<HybridCacheEntryOptions>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>())
            .Returns(emptyResponse);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _cache.Received(1).GetOrCreateAsync<AppointmentsTimelineResponse>(
            expectedKey,
            Arg.Any<Func<CancellationToken, ValueTask<AppointmentsTimelineResponse>>>(),
            Arg.Any<HybridCacheEntryOptions>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithMonthGroupBy_ShouldUseMonthCacheKey()
    {
        // Arrange
        var from = new DateOnly(2026, 3, 1);
        var to = new DateOnly(2026, 3, 31);
        var query = new GetAppointmentsTimelineQuery(from, to, "month");
        var expectedKey = $"appointments-timeline:{_tenantId}:{from}:{to}:month";
        var emptyResponse = new AppointmentsTimelineResponse([]);

        _cache.GetOrCreateAsync<AppointmentsTimelineResponse>(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<AppointmentsTimelineResponse>>>(),
                Arg.Any<HybridCacheEntryOptions>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>())
            .Returns(emptyResponse);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _cache.Received(1).GetOrCreateAsync<AppointmentsTimelineResponse>(
            expectedKey,
            Arg.Any<Func<CancellationToken, ValueTask<AppointmentsTimelineResponse>>>(),
            Arg.Any<HybridCacheEntryOptions>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldTagCacheWithTenantAndReports()
    {
        // Arrange
        var query = new GetAppointmentsTimelineQuery(
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 1, 31));
        var emptyResponse = new AppointmentsTimelineResponse([]);

        IEnumerable<string>? capturedTags = null;
        _cache.GetOrCreateAsync<AppointmentsTimelineResponse>(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<AppointmentsTimelineResponse>>>(),
                Arg.Any<HybridCacheEntryOptions>(),
                Arg.Do<IEnumerable<string>?>(tags => capturedTags = tags),
                Arg.Any<CancellationToken>())
            .Returns(emptyResponse);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        capturedTags.Should().Contain($"tenant:{_tenantId}");
        capturedTags.Should().Contain("reports");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessResult()
    {
        // Arrange
        var query = new GetAppointmentsTimelineQuery(
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 1, 31));
        var response = new AppointmentsTimelineResponse([]);

        _cache.GetOrCreateAsync<AppointmentsTimelineResponse>(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<AppointmentsTimelineResponse>>>(),
                Arg.Any<HybridCacheEntryOptions>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(response);
    }
}
