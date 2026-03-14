namespace AgendeAqui.Domain.Common;

public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "A null value was provided.");

    public bool IsNotFound => Code.EndsWith(".NotFound", StringComparison.Ordinal);
    public bool IsConflict => Code.EndsWith(".Conflict", StringComparison.Ordinal)
        || Code.EndsWith(".InvalidTransition", StringComparison.Ordinal);
    public bool IsNotAuthorized => Code.EndsWith(".NotAuthorized", StringComparison.Ordinal);
}
