using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Reports.GetRevenueTimeline;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using FluentAssertions;
using Microsoft.Extensions.Caching.Hybrid;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Reports;

public class GetRevenueTimelineQueryHandlerTests
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly HybridCache _cache;
    private readonly GetRevenueTimelineQueryHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public GetRevenueTimelineQueryHandlerTests()
    {
        _sqlConnectionFactory = Substitute.For<ISqlConnectionFactory>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _cache = Substitute.For<HybridCache>();
        _tenantProvider.GetTenantId().Returns(_tenantId);
        _handler = new GetRevenueTimelineQueryHandler(_sqlConnectionFactory, _tenantProvider, _cache);
    }

    [Fact]
    public async Task Handle_WhenCacheHit_ShouldReturnCachedResponse()
    {
        // Arrange
        var query = new GetRevenueTimelineQuery(
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 6, 30));
        var cachedResponse = new RevenueTimelineResponse(
            [new RevenuePoint("2026-01", 1500.50m, 30), new RevenuePoint("2026-02", 2000m, 40)]);

        _cache.GetOrCreateAsync<RevenueTimelineResponse>(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<RevenueTimelineResponse>>>(),
                Arg.Any<HybridCacheEntryOptions>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>())
            .Returns(cachedResponse);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Points.Should().HaveCount(2);
        result.Value.Points[0].Month.Should().Be("2026-01");
        result.Value.Points[0].Revenue.Should().Be(1500.50m);
        result.Value.Points[0].AppointmentCount.Should().Be(30);
    }

    [Fact]
    public async Task Handle_ShouldUseTenantScopedCacheKey()
    {
        // Arrange
        var from = new DateOnly(2026, 2, 1);
        var to = new DateOnly(2026, 2, 28);
        var query = new GetRevenueTimelineQuery(from, to);
        var expectedKey = $"revenue-timeline:{_tenantId}:{from}:{to}";
        var emptyResponse = new RevenueTimelineResponse([]);

        _cache.GetOrCreateAsync<RevenueTimelineResponse>(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<RevenueTimelineResponse>>>(),
                Arg.Any<HybridCacheEntryOptions>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>())
            .Returns(emptyResponse);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _cache.Received(1).GetOrCreateAsync<RevenueTimelineResponse>(
            expectedKey,
            Arg.Any<Func<CancellationToken, ValueTask<RevenueTimelineResponse>>>(),
            Arg.Any<HybridCacheEntryOptions>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldTagCacheWithTenantAndReports()
    {
        // Arrange
        var query = new GetRevenueTimelineQuery(
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 1, 31));
        var emptyResponse = new RevenueTimelineResponse([]);

        IEnumerable<string>? capturedTags = null;
        _cache.GetOrCreateAsync<RevenueTimelineResponse>(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<RevenueTimelineResponse>>>(),
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
        var query = new GetRevenueTimelineQuery(
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 1, 31));
        var response = new RevenueTimelineResponse([]);

        _cache.GetOrCreateAsync<RevenueTimelineResponse>(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<RevenueTimelineResponse>>>(),
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
