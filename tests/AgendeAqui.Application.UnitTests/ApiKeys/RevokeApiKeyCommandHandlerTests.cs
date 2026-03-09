using AgendeAqui.Application.ApiKeys.RevokeApiKey;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.ApiKeys;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.ApiKeys;

public class RevokeApiKeyCommandHandlerTests
{
    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RevokeApiKeyCommandHandler _handler;

    public RevokeApiKeyCommandHandlerTests()
    {
        _apiKeyRepository = Substitute.For<IApiKeyRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new RevokeApiKeyCommandHandler(_apiKeyRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithExistingApiKey_RevokesSuccessfully()
    {
        var apiKeyId = Guid.NewGuid();
        var apiKeyResult = ApiKey.Create(Guid.NewGuid(), "Test Key", "somehash");
        var apiKey = apiKeyResult.Value;

        _apiKeyRepository.GetByIdAsync(apiKeyId, Arg.Any<CancellationToken>()).Returns(apiKey);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new RevokeApiKeyCommand(apiKeyId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        apiKey.IsActive.Should().BeFalse();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistingApiKey_ReturnsNotFound()
    {
        var apiKeyId = Guid.NewGuid();
        _apiKeyRepository.GetByIdAsync(apiKeyId, Arg.Any<CancellationToken>()).Returns((ApiKey?)null);

        var result = await _handler.Handle(new RevokeApiKeyCommand(apiKeyId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApiKeyErrors.NotFound);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
