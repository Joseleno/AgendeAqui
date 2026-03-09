using AgendeAqui.Domain.ApiKeys;
using FluentAssertions;

namespace AgendeAqui.Domain.UnitTests.ApiKeys;

public class ApiKeyTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private const string Name = "Integration Key";
    private const string KeyHash = "abc123hash";

    [Fact]
    public void Create_WithValidData_ShouldReturnActiveApiKey()
    {
        var result = ApiKey.Create(TenantId, Name, KeyHash);

        result.IsSuccess.Should().BeTrue();
        result.Value.TenantId.Should().Be(TenantId);
        result.Value.Name.Should().Be(Name);
        result.Value.KeyHash.Should().Be(KeyHash);
        result.Value.IsActive.Should().BeTrue();
        result.Value.ExpiresAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithExpirationDate_ShouldSetExpiresAt()
    {
        var expiresAt = DateTime.UtcNow.AddDays(30);

        var result = ApiKey.Create(TenantId, Name, KeyHash, expiresAt);

        result.IsSuccess.Should().BeTrue();
        result.Value.ExpiresAt.Should().Be(expiresAt);
    }

    [Fact]
    public void Create_WithNullName_ShouldReturnEmptyNameError()
    {
        var result = ApiKey.Create(TenantId, null!, KeyHash);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApiKeyErrors.EmptyName);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldReturnEmptyNameError()
    {
        var result = ApiKey.Create(TenantId, "", KeyHash);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApiKeyErrors.EmptyName);
    }

    [Fact]
    public void Create_WithWhitespaceName_ShouldReturnEmptyNameError()
    {
        var result = ApiKey.Create(TenantId, "   ", KeyHash);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApiKeyErrors.EmptyName);
    }

    [Fact]
    public void Create_WithEmptyKeyHash_ShouldReturnEmptyKeyHashError()
    {
        var result = ApiKey.Create(TenantId, Name, "");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApiKeyErrors.EmptyKeyHash);
    }

    [Fact]
    public void Create_WithNullKeyHash_ShouldReturnEmptyKeyHashError()
    {
        var result = ApiKey.Create(TenantId, Name, null!);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApiKeyErrors.EmptyKeyHash);
    }

    [Fact]
    public void Create_WithWhitespaceKeyHash_ShouldReturnEmptyKeyHashError()
    {
        var result = ApiKey.Create(TenantId, Name, "   ");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApiKeyErrors.EmptyKeyHash);
    }

    [Fact]
    public void Revoke_ShouldSetIsActiveToFalseAndUpdateTimestamp()
    {
        var apiKey = ApiKey.Create(TenantId, Name, KeyHash).Value;
        var before = DateTime.UtcNow.AddSeconds(-1);

        apiKey.Revoke();

        apiKey.IsActive.Should().BeFalse();
        apiKey.UpdatedAt.Should().NotBeNull();
        apiKey.UpdatedAt.Should().BeAfter(before);
    }

    [Fact]
    public void Create_ShouldGenerateNewId()
    {
        var apiKey1 = ApiKey.Create(TenantId, Name, KeyHash).Value;
        var apiKey2 = ApiKey.Create(TenantId, Name, KeyHash).Value;

        apiKey1.Id.Should().NotBe(apiKey2.Id);
    }

    [Fact]
    public void Create_ShouldSetCreatedAt()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);

        var apiKey = ApiKey.Create(TenantId, Name, KeyHash).Value;

        apiKey.CreatedAt.Should().BeAfter(before);
    }
}
