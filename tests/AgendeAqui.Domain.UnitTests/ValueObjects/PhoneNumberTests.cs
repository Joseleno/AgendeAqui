using AgendeAqui.Domain.ValueObjects;
using FluentAssertions;

namespace AgendeAqui.Domain.UnitTests.ValueObjects;

public class PhoneNumberTests
{
    [Theory]
    [InlineData("5511987654321")]   // cell phone (9 digits)
    [InlineData("5521912345678")]   // different DDD
    [InlineData("551198765432")]    // landline (8 digits)
    [InlineData("+55 (11) 98765-4321")]  // formatted cell phone
    [InlineData("55 11 9876-5432")]      // formatted landline
    public void Create_WithValidBrazilianPhone_ShouldSucceed(string phone)
    {
        var result = PhoneNumber.Create(phone);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldFormat_PhoneWithPlusPrefix()
    {
        var result = PhoneNumber.Create("5511987654321");

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("+5511987654321");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithNullOrEmpty_ShouldFail(string? phone)
    {
        var result = PhoneNumber.Create(phone!);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PhoneNumber.InvalidPhone);
    }

    [Theory]
    [InlineData("11987654321")]       // missing country code 55
    [InlineData("1187654321")]        // missing country code + short DDD
    [InlineData("55119876")]          // too few digits
    [InlineData("5511987654321234")]  // too many digits
    [InlineData("not-a-phone")]       // not numeric
    public void Create_WithInvalidPhone_ShouldFail(string phone)
    {
        var result = PhoneNumber.Create(phone);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PhoneNumber.InvalidPhone);
    }

    [Fact]
    public void Equals_WithSamePhoneNumber_ShouldBeEqual()
    {
        var phone1 = PhoneNumber.Create("5511987654321").Value;
        var phone2 = PhoneNumber.Create("+55 (11) 98765-4321").Value;

        phone1.Should().Be(phone2);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedPhoneValue()
    {
        var phone = PhoneNumber.Create("5511987654321").Value;

        phone.ToString().Should().Be("+5511987654321");
    }
}
