using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Professionals;

public sealed class Specialty : ValueObject
{
    public const int MaxLength = 100;

    public string Value { get; }

    private Specialty(string value) => Value = value;

    public static Result<Specialty> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Specialty>(ProfessionalErrors.InvalidSpecialty);

        var trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
            return Result.Failure<Specialty>(ProfessionalErrors.SpecialtyTooLong);

        return Result.Success(new Specialty(trimmed));
    }

    internal static Specialty Hydrate(string value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }

    public override string ToString() => Value;
}
