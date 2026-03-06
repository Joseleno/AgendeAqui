using AgendeAqui.Domain.ValueObjects;
using FluentAssertions;

namespace AgendeAqui.Domain.UnitTests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("user.name+tag@domain.co.br")]
    [InlineData("USER@DOMAIN.COM")]
    [InlineData("valid-email@sub.domain.org")]
    public void Create_WithValidEmail_ShouldSucceed(string email)
    {
        var result = Email.Create(email);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Theory]
    [InlineData("USER@DOMAIN.COM")]
    [InlineData("User@Example.Com")]
    public void Create_ShouldNormalize_EmailToLowercase(string email)
    {
        var result = Email.Create(email);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(email.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithNullOrEmpty_ShouldFail(string? email)
    {
        var result = Email.Create(email!);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Email.InvalidEmail);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    [InlineData("user@domain")]
    [InlineData("user @domain.com")]
    public void Create_WithInvalidFormat_ShouldFail(string email)
    {
        var result = Email.Create(email);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Email.InvalidEmail);
    }

    [Fact]
    public void Equals_WithSameEmail_ShouldBeEqual()
    {
        var email1 = Email.Create("user@example.com").Value;
        var email2 = Email.Create("USER@EXAMPLE.COM").Value;

        email1.Should().Be(email2);
    }

    [Fact]
    public void ToString_ShouldReturnEmailValue()
    {
        var email = Email.Create("user@example.com").Value;

        email.ToString().Should().Be("user@example.com");
    }
}
