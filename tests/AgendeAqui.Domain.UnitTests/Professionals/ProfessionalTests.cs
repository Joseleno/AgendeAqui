using AgendeAqui.Domain.Professionals;
using AgendeAqui.Domain.ValueObjects;
using FluentAssertions;

namespace AgendeAqui.Domain.UnitTests.Professionals;

public class ProfessionalTests
{
    private static Email ValidEmail() => Email.Create("john@example.com").Value;
    private static PhoneNumber ValidPhone() => PhoneNumber.Create("5511987654321").Value;

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var result = Professional.Create(Guid.NewGuid(), "John Doe", ValidEmail(), ValidPhone());

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("John Doe");
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyName_ShouldFail()
    {
        var result = Professional.Create(Guid.NewGuid(), "", ValidEmail(), ValidPhone());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProfessionalErrors.InvalidName);
    }

    [Fact]
    public void Create_WithWhitespaceName_ShouldFail()
    {
        var result = Professional.Create(Guid.NewGuid(), "   ", ValidEmail(), ValidPhone());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProfessionalErrors.InvalidName);
    }

    [Fact]
    public void Create_ShouldTrimName()
    {
        var result = Professional.Create(Guid.NewGuid(), "  John Doe  ", ValidEmail(), ValidPhone());

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("John Doe");
    }

    [Fact]
    public void Activate_ShouldSetIsActiveAndUpdatedAt()
    {
        var professional = Professional.Create(Guid.NewGuid(), "John", ValidEmail(), ValidPhone()).Value;
        professional.Deactivate();

        professional.Activate();

        professional.IsActive.Should().BeTrue();
        professional.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_ShouldSetIsInactiveAndUpdatedAt()
    {
        var professional = Professional.Create(Guid.NewGuid(), "John", ValidEmail(), ValidPhone()).Value;

        professional.Deactivate();

        professional.IsActive.Should().BeFalse();
        professional.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateContact_WithValidData_ShouldSucceed()
    {
        var professional = Professional.Create(Guid.NewGuid(), "John", ValidEmail(), ValidPhone()).Value;
        var newEmail = Email.Create("jane@example.com").Value;
        var newPhone = PhoneNumber.Create("5521912345678").Value;

        var result = professional.UpdateContact("Jane Doe", newEmail, newPhone);

        result.IsSuccess.Should().BeTrue();
        professional.Name.Should().Be("Jane Doe");
        professional.Email.Should().Be(newEmail);
        professional.Phone.Should().Be(newPhone);
        professional.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateContact_WithEmptyName_ShouldFail()
    {
        var professional = Professional.Create(Guid.NewGuid(), "John", ValidEmail(), ValidPhone()).Value;

        var result = professional.UpdateContact("", ValidEmail(), ValidPhone());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProfessionalErrors.InvalidName);
        professional.Name.Should().Be("John");
    }
}
