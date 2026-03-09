using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Common;
using AgendeAqui.Application.Services.GetService;
using AgendeAqui.Application.Services.ListServices;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using FluentAssertions;
using Microsoft.Extensions.Caching.Hybrid;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Services;

public class ListServicesQueryHandlerTests
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly HybridCache _cache;
    private readonly ListServicesQueryHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public ListServicesQueryHandlerTests()
    {
        _sqlConnectionFactory = Substitute.For<ISqlConnectionFactory>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _cache = Substitute.For<HybridCache>();
        _tenantProvider.GetTenantId().Returns(_tenantId);
        _handler = new ListServicesQueryHandler(_sqlConnectionFactory, _tenantProvider, _cache);
    }

    [Fact]
    public async Task Handle_WhenCacheHit_ShouldReturnCachedPagedResponse()
    {
        // Arrange
        var query = new ListServicesQuery(1, 10);
        var cachedResponse = new PagedResponse<ServiceResponse>(
            [new ServiceResponse(Guid.NewGuid(), "Corte de Cabelo", 30, 50m, true, DateTime.UtcNow)],
            1, 10, 1);

        _cache.GetOrCreateAsync<PagedResponse<ServiceResponse>>(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<PagedResponse<ServiceResponse>>>>(),
                Arg.Any<HybridCacheEntryOptions>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>())
            .Returns(cachedResponse);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
        result.Value.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldUseTenantScopedCacheKey()
    {
        // Arrange
        var query = new ListServicesQuery(2, 20);
        var expectedKey = $"services:{_tenantId}:p2:s20";
        var emptyResponse = new PagedResponse<ServiceResponse>([], 2, 20, 0);

        _cache.GetOrCreateAsync<PagedResponse<ServiceResponse>>(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<PagedResponse<ServiceResponse>>>>(),
                Arg.Any<HybridCacheEntryOptions>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>())
            .Returns(emptyResponse);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _cache.Received(1).GetOrCreateAsync<PagedResponse<ServiceResponse>>(
            expectedKey,
            Arg.Any<Func<CancellationToken, ValueTask<PagedResponse<ServiceResponse>>>>(),
            Arg.Any<HybridCacheEntryOptions>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldTagCacheWithTenantAndDomain()
    {
        // Arrange
        var query = new ListServicesQuery(1, 10);
        var emptyResponse = new PagedResponse<ServiceResponse>([], 1, 10, 0);

        IEnumerable<string>? capturedTags = null;
        _cache.GetOrCreateAsync<PagedResponse<ServiceResponse>>(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<PagedResponse<ServiceResponse>>>>(),
                Arg.Any<HybridCacheEntryOptions>(),
                Arg.Do<IEnumerable<string>?>(tags => capturedTags = tags),
                Arg.Any<CancellationToken>())
            .Returns(emptyResponse);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        capturedTags.Should().Contain($"tenant:{_tenantId}");
        capturedTags.Should().Contain("services");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessResult()
    {
        // Arrange
        var query = new ListServicesQuery(1, 10);
        var response = new PagedResponse<ServiceResponse>([], 1, 10, 0);

        _cache.GetOrCreateAsync<PagedResponse<ServiceResponse>>(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<PagedResponse<ServiceResponse>>>>(),
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
