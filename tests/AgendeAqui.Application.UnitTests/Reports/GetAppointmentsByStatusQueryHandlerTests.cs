using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Reports.GetAppointmentsByStatus;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using FluentAssertions;
using Microsoft.Extensions.Caching.Hybrid;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Reports;

public class GetAppointmentsByStatusQueryHandlerTests
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly HybridCache _cache;
    private readonly GetAppointmentsByStatusQueryHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public GetAppointmentsByStatusQueryHandlerTests()
    {
        _sqlConnectionFactory = Substitute.For<ISqlConnectionFactory>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _cache = Substitute.For<HybridCache>();
        _tenantProvider.GetTenantId().Returns(_tenantId);
        _handler = new GetAppointmentsByStatusQueryHandler(_sqlConnectionFactory, _tenantProvider, _cache);
    }

    [Fact]
    public async Task Handle_WhenCacheHit_ShouldReturnCachedResponse()
    {
        // Arrange
        var query = new GetAppointmentsByStatusQuery(
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 1, 31));
        var cachedResponse = new AppointmentsByStatusResponse(
            [new StatusCount("Completed", 10), new StatusCount("Cancelled", 3)]);

        _cache.GetOrCreateAsync<AppointmentsByStatusResponse>(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<AppointmentsByStatusResponse>>>(),
                Arg.Any<HybridCacheEntryOptions>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>())
            .Returns(cachedResponse);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items[0].Status.Should().Be("Completed");
        result.Value.Items[0].Count.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ShouldUseTenantScopedCacheKey()
    {
        // Arrange
        var from = new DateOnly(2026, 2, 1);
        var to = new DateOnly(2026, 2, 28);
        var query = new GetAppointmentsByStatusQuery(from, to);
        var expectedKey = $"appointments-by-status:{_tenantId}:{from}:{to}";
        var emptyResponse = new AppointmentsByStatusResponse([]);

        _cache.GetOrCreateAsync<AppointmentsByStatusResponse>(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<AppointmentsByStatusResponse>>>(),
                Arg.Any<HybridCacheEntryOptions>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>())
            .Returns(emptyResponse);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _cache.Received(1).GetOrCreateAsync<AppointmentsByStatusResponse>(
            expectedKey,
            Arg.Any<Func<CancellationToken, ValueTask<AppointmentsByStatusResponse>>>(),
            Arg.Any<HybridCacheEntryOptions>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldTagCacheWithTenantAndReports()
    {
        // Arrange
        var query = new GetAppointmentsByStatusQuery(
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 1, 31));
        var emptyResponse = new AppointmentsByStatusResponse([]);

        IEnumerable<string>? capturedTags = null;
        _cache.GetOrCreateAsync<AppointmentsByStatusResponse>(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<AppointmentsByStatusResponse>>>(),
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
        var query = new GetAppointmentsByStatusQuery(
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 1, 31));
        var response = new AppointmentsByStatusResponse([]);

        _cache.GetOrCreateAsync<AppointmentsByStatusResponse>(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<AppointmentsByStatusResponse>>>(),
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
