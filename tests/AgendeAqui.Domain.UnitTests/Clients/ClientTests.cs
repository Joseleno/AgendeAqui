using AgendeAqui.Domain.Clients;
using AgendeAqui.Domain.ValueObjects;
using FluentAssertions;

namespace AgendeAqui.Domain.UnitTests.Clients;

public class ClientTests
{
    private static Email ValidEmail() => Email.Create("client@example.com").Value;
    private static PhoneNumber ValidPhone() => PhoneNumber.Create("5511987654321").Value;

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var result = Client.Create(Guid.NewGuid(), "Maria Silva", ValidEmail(), ValidPhone());

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Maria Silva");
    }

    [Fact]
    public void Create_WithEmptyName_ShouldFail()
    {
        var result = Client.Create(Guid.NewGuid(), "", ValidEmail(), ValidPhone());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ClientErrors.InvalidName);
    }

    [Fact]
    public void Create_WithWhitespaceName_ShouldFail()
    {
        var result = Client.Create(Guid.NewGuid(), "   ", ValidEmail(), ValidPhone());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ClientErrors.InvalidName);
    }

    [Fact]
    public void Create_ShouldTrimName()
    {
        var result = Client.Create(Guid.NewGuid(), "  Maria Silva  ", ValidEmail(), ValidPhone());

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Maria Silva");
    }

    [Fact]
    public void UpdateContact_ShouldUpdateAllFields()
    {
        var client = Client.Create(Guid.NewGuid(), "Maria", ValidEmail(), ValidPhone()).Value;
        var newEmail = Email.Create("new@example.com").Value;
        var newPhone = PhoneNumber.Create("5521912345678").Value;

        client.UpdateContact("Maria Updated", newEmail, newPhone);

        client.Name.Should().Be("Maria Updated");
        client.Email.Should().Be(newEmail);
        client.Phone.Should().Be(newPhone);
        client.UpdatedAt.Should().NotBeNull();
    }
}
