using AgendeAqui.Application.Behaviors;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Mediator;
using NSubstitute;
using ValidationException = AgendeAqui.Application.Exceptions.ValidationException;

namespace AgendeAqui.Application.UnitTests.Behaviors;

public class ValidationBehaviorTests
{
    public record TestRequest(string Value) : IMessage;

    [Fact]
    public async Task Handle_WithNoValidators_ShouldCallNext()
    {
        var validators = Enumerable.Empty<IValidator<TestRequest>>();
        var behavior = new ValidationBehavior<TestRequest, string>(validators);
        var request = new TestRequest("valid");
        var nextCalled = false;

        MessageHandlerDelegate<TestRequest, string> next = (_, _) =>
        {
            nextCalled = true;
            return ValueTask.FromResult("result");
        };

        var result = await behavior.Handle(request, next, CancellationToken.None);

        nextCalled.Should().BeTrue();
        result.Should().Be("result");
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCallNext()
    {
        var validator = Substitute.For<IValidator<TestRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var validators = new[] { validator };
        var behavior = new ValidationBehavior<TestRequest, string>(validators);
        var request = new TestRequest("valid");
        var nextCalled = false;

        MessageHandlerDelegate<TestRequest, string> next = (_, _) =>
        {
            nextCalled = true;
            return ValueTask.FromResult("result");
        };

        var result = await behavior.Handle(request, next, CancellationToken.None);

        nextCalled.Should().BeTrue();
        result.Should().Be("result");
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ShouldThrowValidationException()
    {
        var failures = new[]
        {
            new ValidationFailure("Value", "Value is required"),
            new ValidationFailure("Value", "Value must not be empty")
        };

        var validationResult = new ValidationResult(failures);

        var validator = Substitute.For<IValidator<TestRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(validationResult);

        var validators = new[] { validator };
        var behavior = new ValidationBehavior<TestRequest, string>(validators);
        var request = new TestRequest("");

        MessageHandlerDelegate<TestRequest, string> next = (_, _) =>
            ValueTask.FromResult("result");

        var act = async () => await behavior.Handle(request, next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*validation*");
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ShouldIncludeAllErrors()
    {
        var failures = new[]
        {
            new ValidationFailure("Name", "Name is required"),
            new ValidationFailure("Slug", "Slug must be lowercase")
        };

        var validationResult = new ValidationResult(failures);

        var validator = Substitute.For<IValidator<TestRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(validationResult);

        var validators = new[] { validator };
        var behavior = new ValidationBehavior<TestRequest, string>(validators);
        var request = new TestRequest("");

        MessageHandlerDelegate<TestRequest, string> next = (_, _) =>
            ValueTask.FromResult("result");

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(request, next, CancellationToken.None).AsTask());

        exception.Errors.Should().HaveCount(2);
        exception.Errors.Should().Contain(e => e.PropertyName == "Name");
        exception.Errors.Should().Contain(e => e.PropertyName == "Slug");
    }

    [Fact]
    public async Task Handle_WithMultipleValidators_ShouldAggregateErrors()
    {
        var validator1 = Substitute.For<IValidator<TestRequest>>();
        validator1.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("Value", "Error from validator 1") }));

        var validator2 = Substitute.For<IValidator<TestRequest>>();
        validator2.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("Value", "Error from validator 2") }));

        var validators = new[] { validator1, validator2 };
        var behavior = new ValidationBehavior<TestRequest, string>(validators);
        var request = new TestRequest("");

        MessageHandlerDelegate<TestRequest, string> next = (_, _) =>
            ValueTask.FromResult("result");

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(request, next, CancellationToken.None).AsTask());

        exception.Errors.Should().HaveCount(2);
    }
}
