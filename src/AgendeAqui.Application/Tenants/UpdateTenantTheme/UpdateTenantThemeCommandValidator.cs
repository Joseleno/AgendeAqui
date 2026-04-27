using FluentValidation;

namespace AgendeAqui.Application.Tenants.UpdateTenantTheme;

public sealed class UpdateTenantThemeCommandValidator : AbstractValidator<UpdateTenantThemeCommand>
{
    public UpdateTenantThemeCommandValidator()
    {
        RuleFor(x => x.PrimaryColor)
            .NotEmpty()
            .MaximumLength(7)
            .Matches("^#[0-9a-fA-F]{6}$")
            .WithMessage("PrimaryColor must be a valid hex color (e.g. #6366f1).");

        RuleFor(x => x.LogoUrl)
            .MaximumLength(2048)
            .Must(url => url is null || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("LogoUrl must be a valid absolute URL.")
            .When(x => x.LogoUrl is not null);

        RuleFor(x => x.FaviconUrl)
            .MaximumLength(2048)
            .Must(url => url is null || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("FaviconUrl must be a valid absolute URL.")
            .When(x => x.FaviconUrl is not null);
    }
}
