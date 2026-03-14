using AgendeAqui.Domain.Professionals;
using FluentAssertions;

namespace AgendeAqui.Domain.UnitTests.Professionals;

public class SpecialtyTests
{
    [Fact]
    public void Create_WithValidString_ShouldSucceed()
    {
        var result = Specialty.Create("Cardiologia");

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("Cardiologia");
    }

    [Fact]
    public void Create_WithEmptyString_ShouldFail()
    {
        var result = Specialty.Create("");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProfessionalErrors.InvalidSpecialty);
    }

    [Fact]
    public void Create_WithWhitespace_ShouldFail()
    {
        var result = Specialty.Create("   ");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProfessionalErrors.InvalidSpecialty);
    }

    [Fact]
    public void Create_WithStringOver100Chars_ShouldFail()
    {
        var longString = new string('A', 101);
        var result = Specialty.Create(longString);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProfessionalErrors.SpecialtyTooLong);
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        var result = Specialty.Create("  Cardiologia  ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("Cardiologia");
    }

    [Fact]
    public void Equality_ShouldBeCaseInsensitive()
    {
        var s1 = Specialty.Create("Cardiology").Value;
        var s2 = Specialty.Create("CARDIOLOGY").Value;

        s1.Should().Be(s2);
    }

    [Fact]
    public void Hydrate_ShouldCreateWithoutValidation()
    {
        var specialty = Specialty.Hydrate("Any value");

        specialty.Value.Should().Be("Any value");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var specialty = Specialty.Create("Dermatologia").Value;

        specialty.ToString().Should().Be("Dermatologia");
    }
}
