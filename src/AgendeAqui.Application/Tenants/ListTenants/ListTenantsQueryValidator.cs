using FluentValidation;

namespace AgendeAqui.Application.Tenants.ListTenants;

public sealed class ListTenantsQueryValidator : AbstractValidator<ListTenantsQuery>
{
    public ListTenantsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50);

        When(x => !string.IsNullOrWhiteSpace(x.Status), () =>
        {
            RuleFor(x => x.Status)
                .Must(s => Enum.TryParse<Domain.Tenants.TenantStatus>(s, true, out _))
                .WithMessage("Invalid status. Valid values: Active, Inactive, Suspended.");
        });
    }
}
