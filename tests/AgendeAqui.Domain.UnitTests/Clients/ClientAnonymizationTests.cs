using AgendeAqui.Domain.Clients;
using AgendeAqui.Domain.ValueObjects;
using FluentAssertions;

namespace AgendeAqui.Domain.UnitTests.Clients;

public class ClientAnonymizationTests
{
    private static Email ValidEmail() => Email.Create("client@example.com").Value;
    private static PhoneNumber ValidPhone() => PhoneNumber.Create("5511987654321").Value;

    private static Client CreateValidClient() =>
        Client.Create(Guid.NewGuid(), "Maria Silva", ValidEmail(), ValidPhone()).Value;

    [Fact]
    public void Anonymize_ShouldSetNameToAnonymized()
    {
        var client = CreateValidClient();

        client.Anonymize();

        client.Name.Should().Be("***ANONYMIZED***");
    }

    [Fact]
    public void Anonymize_ShouldSetEmailToAnonymized()
    {
        var client = CreateValidClient();

        client.Anonymize();

        client.Email.Value.Should().Be("anonymized@removed.com");
    }

    [Fact]
    public void Anonymize_ShouldSetPhoneToAnonymized()
    {
        var client = CreateValidClient();

        client.Anonymize();

        client.Phone.Value.Should().Be("+5500000000000");
    }

    [Fact]
    public void Anonymize_ShouldUpdateUpdatedAt()
    {
        var client = CreateValidClient();
        var before = DateTime.UtcNow.AddSeconds(-1);

        client.Anonymize();

        client.UpdatedAt.Should().NotBeNull();
        client.UpdatedAt.Should().BeAfter(before);
    }

    [Fact]
    public void Anonymize_CalledTwice_ShouldBeIdempotent()
    {
        var client = CreateValidClient();

        client.Anonymize();
        client.Anonymize();

        client.Name.Should().Be("***ANONYMIZED***");
        client.Email.Value.Should().Be("anonymized@removed.com");
        client.Phone.Value.Should().Be("+5500000000000");
    }

    [Fact]
    public void IsAnonymized_BeforeAnonymize_ShouldBeFalse()
    {
        var client = CreateValidClient();

        client.IsAnonymized.Should().BeFalse();
    }

    [Fact]
    public void IsAnonymized_AfterAnonymize_ShouldBeTrue()
    {
        var client = CreateValidClient();

        client.Anonymize();

        client.IsAnonymized.Should().BeTrue();
    }
}
