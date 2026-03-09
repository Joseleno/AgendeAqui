using AgendeAqui.Application.ApiKeys.CreateApiKey;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.ApiKeys;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.ApiKeys;

public class CreateApiKeyCommandHandlerTests
{
    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly CreateApiKeyCommandHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public CreateApiKeyCommandHandlerTests()
    {
        _apiKeyRepository = Substitute.For<IApiKeyRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _tenantProvider.GetTenantId().Returns(_tenantId);
        _handler = new CreateApiKeyCommandHandler(_apiKeyRepository, _unitOfWork, _tenantProvider);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsRawKeyAndId()
    {
        var command = new CreateApiKeyCommand("My API Key", DateTime.UtcNow.AddDays(30));
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.RawKey.Should().StartWith("sk_live_");
        result.Value.RawKey.Should().HaveLength(72); // "sk_live_" (8) + 64 hex chars
        result.Value.Name.Should().Be("My API Key");
        result.Value.Id.Should().NotBeEmpty();
        await _apiKeyRepository.Received(1).AddAsync(Arg.Any<ApiKey>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptyName_ReturnsFailure()
    {
        var command = new CreateApiKeyCommand("", null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApiKeyErrors.EmptyName);
        await _apiKeyRepository.DidNotReceive().AddAsync(Arg.Any<ApiKey>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GeneratesUniqueKeysPerCall()
    {
        var command = new CreateApiKeyCommand("Key", null);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result1 = await _handler.Handle(command, CancellationToken.None);
        var result2 = await _handler.Handle(command, CancellationToken.None);

        result1.Value.RawKey.Should().NotBe(result2.Value.RawKey);
    }
}
