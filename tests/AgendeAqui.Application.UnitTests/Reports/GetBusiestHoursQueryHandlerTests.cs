using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Reports.GetBusiestHours;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using FluentAssertions;
using Microsoft.Extensions.Caching.Hybrid;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Reports;

public class GetBusiestHoursQueryHandlerTests
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly HybridCache _cache;
    private readonly GetBusiestHoursQueryHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public GetBusiestHoursQueryHandlerTests()
    {
        _sqlConnectionFactory = Substitute.For<ISqlConnectionFactory>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _cache = Substitute.For<HybridCache>();
        _tenantProvider.GetTenantId().Returns(_tenantId);
        _handler = new GetBusiestHoursQueryHandler(_sqlConnectionFactory, _tenantProvider, _cache);
    }

    [Fact]
    public async Task Handle_WhenCacheHit_ShouldReturnCachedResponse()
    {
        // Arrange
        var query = new GetBusiestHoursQuery(
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 1, 31));
        var cachedResponse = new BusiestHoursResponse(
            [new HourSlot(1, 9, 15), new HourSlot(1, 10, 20)]);

        _cache.GetOrCreateAsync<BusiestHoursResponse>(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<BusiestHoursResponse>>>(),
                Arg.Any<HybridCacheEntryOptions>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>())
            .Returns(cachedResponse);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Slots.Should().HaveCount(2);
        result.Value.Slots[0].DayOfWeek.Should().Be(1);
        result.Value.Slots[0].Hour.Should().Be(9);
        result.Value.Slots[0].Count.Should().Be(15);
    }

    [Fact]
    public async Task Handle_ShouldUseTenantScopedCacheKey()
    {
        // Arrange
        var from = new DateOnly(2026, 2, 1);
        var to = new DateOnly(2026, 2, 28);
        var query = new GetBusiestHoursQuery(from, to);
        var expectedKey = $"busiest-hours:{_tenantId}:{from}:{to}";
        var emptyResponse = new BusiestHoursResponse([]);

        _cache.GetOrCreateAsync<BusiestHoursResponse>(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<BusiestHoursResponse>>>(),
                Arg.Any<HybridCacheEntryOptions>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>())
            .Returns(emptyResponse);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _cache.Received(1).GetOrCreateAsync<BusiestHoursResponse>(
            expectedKey,
            Arg.Any<Func<CancellationToken, ValueTask<BusiestHoursResponse>>>(),
            Arg.Any<HybridCacheEntryOptions>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldTagCacheWithTenantAndReports()
    {
        // Arrange
        var query = new GetBusiestHoursQuery(
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 1, 31));
        var emptyResponse = new BusiestHoursResponse([]);

        IEnumerable<string>? capturedTags = null;
        _cache.GetOrCreateAsync<BusiestHoursResponse>(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<BusiestHoursResponse>>>(),
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
        var query = new GetBusiestHoursQuery(
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 1, 31));
        var response = new BusiestHoursResponse([]);

        _cache.GetOrCreateAsync<BusiestHoursResponse>(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<BusiestHoursResponse>>>(),
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
