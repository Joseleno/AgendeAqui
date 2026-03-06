using FluentValidation.Results;

namespace AgendeAqui.Application.Exceptions;

public sealed class ValidationException(IEnumerable<ValidationFailure> failures) : Exception("One or more validation failures occurred.")
{
    public IReadOnlyCollection<ValidationError> Errors { get; } = failures
        .Select(f => new ValidationError(f.PropertyName, f.ErrorMessage))
        .ToList()
        .AsReadOnly();
}

public sealed record ValidationError(string PropertyName, string ErrorMessage);
