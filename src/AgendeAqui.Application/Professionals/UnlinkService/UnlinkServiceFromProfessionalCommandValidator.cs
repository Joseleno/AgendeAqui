using FluentValidation;

namespace AgendeAqui.Application.Professionals.UnlinkService;

public sealed class UnlinkServiceFromProfessionalCommandValidator : AbstractValidator<UnlinkServiceFromProfessionalCommand>
{
    public UnlinkServiceFromProfessionalCommandValidator()
    {
        RuleFor(x => x.ProfessionalId).NotEmpty();
        RuleFor(x => x.ServiceId).NotEmpty();
    }
}
