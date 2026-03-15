using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Reports.GetPatientGrowth;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using FluentAssertions;
using Microsoft.Extensions.Caching.Hybrid;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Reports;

public class GetPatientGrowthQueryHandlerTests
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly HybridCache _cache;
    private readonly GetPatientGrowthQueryHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public GetPatientGrowthQueryHandlerTests()
    {
        _sqlConnectionFactory = Substitute.For<ISqlConnectionFactory>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _cache = Substitute.For<HybridCache>();
        _tenantProvider.GetTenantId().Returns(_tenantId);
        _handler = new GetPatientGrowthQueryHandler(_sqlConnectionFactory, _tenantProvider, _cache);
    }

    [Fact]
    public async Task Handle_WhenCacheHit_ShouldReturnCachedResponse()
    {
        // Arrange
        var query = new GetPatientGrowthQuery(6);
        var cachedResponse = new PatientGrowthResponse(
            [new GrowthPoint("2026-01", 5, 5), new GrowthPoint("2026-02", 3, 8)]);

        _cache.GetOrCreateAsync<PatientGrowthResponse>(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<PatientGrowthResponse>>>(),
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
        result.Value.Points[0].NewClients.Should().Be(5);
        result.Value.Points[0].TotalClients.Should().Be(5);
        result.Value.Points[1].TotalClients.Should().Be(8);
    }

    [Fact]
    public async Task Handle_ShouldUseTenantScopedCacheKey()
    {
        // Arrange
        var query = new GetPatientGrowthQuery(12);
        var expectedKey = $"patient-growth:{_tenantId}:12";
        var emptyResponse = new PatientGrowthResponse([]);

        _cache.GetOrCreateAsync<PatientGrowthResponse>(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<PatientGrowthResponse>>>(),
                Arg.Any<HybridCacheEntryOptions>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>())
            .Returns(emptyResponse);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _cache.Received(1).GetOrCreateAsync<PatientGrowthResponse>(
            expectedKey,
            Arg.Any<Func<CancellationToken, ValueTask<PatientGrowthResponse>>>(),
            Arg.Any<HybridCacheEntryOptions>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithCustomMonths_ShouldIncludeMonthsInCacheKey()
    {
        // Arrange
        var query = new GetPatientGrowthQuery(3);
        var expectedKey = $"patient-growth:{_tenantId}:3";
        var emptyResponse = new PatientGrowthResponse([]);

        _cache.GetOrCreateAsync<PatientGrowthResponse>(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<PatientGrowthResponse>>>(),
                Arg.Any<HybridCacheEntryOptions>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>())
            .Returns(emptyResponse);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _cache.Received(1).GetOrCreateAsync<PatientGrowthResponse>(
            expectedKey,
            Arg.Any<Func<CancellationToken, ValueTask<PatientGrowthResponse>>>(),
            Arg.Any<HybridCacheEntryOptions>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldTagCacheWithTenantAndReports()
    {
        // Arrange
        var query = new GetPatientGrowthQuery();
        var emptyResponse = new PatientGrowthResponse([]);

        IEnumerable<string>? capturedTags = null;
        _cache.GetOrCreateAsync<PatientGrowthResponse>(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<PatientGrowthResponse>>>(),
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
        var query = new GetPatientGrowthQuery();
        var response = new PatientGrowthResponse([]);

        _cache.GetOrCreateAsync<PatientGrowthResponse>(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<PatientGrowthResponse>>>(),
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
