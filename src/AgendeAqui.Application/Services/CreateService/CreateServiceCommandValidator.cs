using FluentValidation;

namespace AgendeAqui.Application.Services.CreateService;

public sealed class CreateServiceCommandValidator : AbstractValidator<CreateServiceCommand>
{
    public CreateServiceCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0)
            .LessThanOrEqualTo(480);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0);
    }
}
