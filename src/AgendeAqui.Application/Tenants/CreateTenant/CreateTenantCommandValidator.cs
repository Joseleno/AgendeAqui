using FluentValidation;

namespace AgendeAqui.Application.Tenants.CreateTenant;

public sealed class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(100)
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug must be lowercase alphanumeric with hyphens only.");

        RuleFor(x => x.Plan)
            .NotEmpty()
            .Must(p => Enum.TryParse<Domain.Tenants.TenantPlan>(p, true, out _))
            .WithMessage("Invalid plan. Valid values: Free, Starter, Professional, Enterprise.");
    }
}
