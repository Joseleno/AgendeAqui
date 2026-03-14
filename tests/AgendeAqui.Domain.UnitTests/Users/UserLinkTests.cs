using AgendeAqui.Domain.Users;
using FluentAssertions;

namespace AgendeAqui.Domain.UnitTests.Users;

public class UserLinkTests
{
    private User CreateUser() =>
        User.Create(Guid.NewGuid(), "test@test.com", "hashed-password", "Test User", "Admin").Value;

    [Fact]
    public void NewUser_ShouldHaveNullProfessionalIdAndClientId()
    {
        var user = CreateUser();

        user.ProfessionalId.Should().BeNull();
        user.ClientId.Should().BeNull();
    }

    [Fact]
    public void LinkToProfessional_WithValidGuid_ShouldSucceed()
    {
        var user = CreateUser();
        var professionalId = Guid.NewGuid();

        var result = user.LinkToProfessional(professionalId);

        result.IsSuccess.Should().BeTrue();
        user.ProfessionalId.Should().Be(professionalId);
    }

    [Fact]
    public void LinkToProfessional_WhenAlreadyLinked_ShouldReturnFailure()
    {
        var user = CreateUser();
        user.LinkToProfessional(Guid.NewGuid());

        var result = user.LinkToProfessional(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.AlreadyLinkedToProfessional);
    }

    [Fact]
    public void LinkToClient_WithValidGuid_ShouldSucceed()
    {
        var user = CreateUser();
        var clientId = Guid.NewGuid();

        var result = user.LinkToClient(clientId);

        result.IsSuccess.Should().BeTrue();
        user.ClientId.Should().Be(clientId);
    }

    [Fact]
    public void LinkToClient_WhenAlreadyLinked_ShouldReturnFailure()
    {
        var user = CreateUser();
        user.LinkToClient(Guid.NewGuid());

        var result = user.LinkToClient(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.AlreadyLinkedToClient);
    }
}
