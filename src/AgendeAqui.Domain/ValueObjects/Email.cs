using System.Text.RegularExpressions;
using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.ValueObjects;

public sealed class Email : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static readonly Error InvalidEmail = new("Email.Invalid", "The email address is not valid.");

    public string Value { get; }

    private Email(string value) => Value = value.ToLowerInvariant();

    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<Email>(InvalidEmail);

        if (!EmailRegex.IsMatch(email))
            return Result.Failure<Email>(InvalidEmail);

        return Result.Success(new Email(email));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
