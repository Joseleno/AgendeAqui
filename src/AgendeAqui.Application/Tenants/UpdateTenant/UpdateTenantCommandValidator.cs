using FluentValidation;

namespace AgendeAqui.Application.Tenants.UpdateTenant;

public sealed class UpdateTenantCommandValidator : AbstractValidator<UpdateTenantCommand>
{
    public UpdateTenantCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Plan)
            .NotEmpty()
            .Must(p => Enum.TryParse<Domain.Tenants.TenantPlan>(p, true, out _))
            .WithMessage("Invalid plan. Valid values: Free, Starter, Professional, Enterprise.");
    }
}
