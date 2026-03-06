using System.Text.RegularExpressions;
using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.ValueObjects;

public sealed class PhoneNumber : ValueObject
{
    private static readonly Regex DigitsOnly = new(@"\D", RegexOptions.Compiled);
    // Brazilian: country code 55 + DDD 2 digits + number 8-9 digits = 12-13 digits total
    private static readonly Regex BrazilianPhone = new(@"^55\d{2}\d{8,9}$", RegexOptions.Compiled);

    public static readonly Error InvalidPhone = new("PhoneNumber.Invalid", "The phone number is not a valid Brazilian phone number.");

    public string Value { get; }

    private PhoneNumber(string value) => Value = value;

    public static Result<PhoneNumber> Create(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return Result.Failure<PhoneNumber>(InvalidPhone);

        var digitsOnly = DigitsOnly.Replace(phone, string.Empty);

        if (!BrazilianPhone.IsMatch(digitsOnly))
            return Result.Failure<PhoneNumber>(InvalidPhone);

        var formatted = $"+{digitsOnly}";
        return Result.Success(new PhoneNumber(formatted));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
