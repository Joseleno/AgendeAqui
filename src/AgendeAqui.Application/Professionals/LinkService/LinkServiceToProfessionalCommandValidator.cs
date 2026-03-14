using FluentValidation;

namespace AgendeAqui.Application.Professionals.LinkService;

public sealed class LinkServiceToProfessionalCommandValidator : AbstractValidator<LinkServiceToProfessionalCommand>
{
    public LinkServiceToProfessionalCommandValidator()
    {
        RuleFor(x => x.ProfessionalId).NotEmpty();
        RuleFor(x => x.ServiceId).NotEmpty();
    }
}
