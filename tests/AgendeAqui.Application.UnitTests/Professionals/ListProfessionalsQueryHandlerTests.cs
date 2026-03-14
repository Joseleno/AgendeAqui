using AgendeAqui.Application.Abstractions.Data;
using AgendeAqui.Application.Common;
using AgendeAqui.Application.Professionals.GetProfessional;
using AgendeAqui.Application.Professionals.ListProfessionals;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using FluentAssertions;
using Microsoft.Extensions.Caching.Hybrid;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Professionals;

public class ListProfessionalsQueryHandlerTests
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ITenantProvider _tenantProvider;
    private readonly HybridCache _cache;
    private readonly ListProfessionalsQueryHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public ListProfessionalsQueryHandlerTests()
    {
        _sqlConnectionFactory = Substitute.For<ISqlConnectionFactory>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _cache = Substitute.For<HybridCache>();
        _tenantProvider.GetTenantId().Returns(_tenantId);
        _handler = new ListProfessionalsQueryHandler(_sqlConnectionFactory, _tenantProvider, _cache);
    }

    [Fact]
    public async Task Handle_WhenCacheHit_ShouldReturnCachedPagedResponse()
    {
        // Arrange
        var query = new ListProfessionalsQuery(1, 10);
        var cachedResponse = new PagedResponse<ProfessionalResponse>(
            [new ProfessionalResponse(Guid.NewGuid(), "Ana Lima", "ana@example.com", "+5511999990001", true, null, DateTime.UtcNow)],
            1, 10, 1);

        _cache.GetOrCreateAsync<PagedResponse<ProfessionalResponse>>(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<PagedResponse<ProfessionalResponse>>>>(),
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
        var query = new ListProfessionalsQuery(3, 15);
        var expectedKey = $"professionals:{_tenantId}:p3:s15";
        var emptyResponse = new PagedResponse<ProfessionalResponse>([], 3, 15, 0);

        _cache.GetOrCreateAsync<PagedResponse<ProfessionalResponse>>(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<PagedResponse<ProfessionalResponse>>>>(),
                Arg.Any<HybridCacheEntryOptions>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>())
            .Returns(emptyResponse);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _cache.Received(1).GetOrCreateAsync<PagedResponse<ProfessionalResponse>>(
            expectedKey,
            Arg.Any<Func<CancellationToken, ValueTask<PagedResponse<ProfessionalResponse>>>>(),
            Arg.Any<HybridCacheEntryOptions>(),
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldTagCacheWithTenantAndDomain()
    {
        // Arrange
        var query = new ListProfessionalsQuery(1, 10);
        var emptyResponse = new PagedResponse<ProfessionalResponse>([], 1, 10, 0);

        IEnumerable<string>? capturedTags = null;
        _cache.GetOrCreateAsync<PagedResponse<ProfessionalResponse>>(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<PagedResponse<ProfessionalResponse>>>>(),
                Arg.Any<HybridCacheEntryOptions>(),
                Arg.Do<IEnumerable<string>?>(tags => capturedTags = tags),
                Arg.Any<CancellationToken>())
            .Returns(emptyResponse);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        capturedTags.Should().Contain($"tenant:{_tenantId}");
        capturedTags.Should().Contain("professionals");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessResult()
    {
        // Arrange
        var query = new ListProfessionalsQuery(1, 10);
        var response = new PagedResponse<ProfessionalResponse>([], 1, 10, 0);

        _cache.GetOrCreateAsync<PagedResponse<ProfessionalResponse>>(
                Arg.Any<string>(),
                Arg.Any<Func<CancellationToken, ValueTask<PagedResponse<ProfessionalResponse>>>>(),
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
