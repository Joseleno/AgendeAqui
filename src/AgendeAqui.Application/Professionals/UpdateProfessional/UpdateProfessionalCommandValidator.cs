using FluentValidation;

namespace AgendeAqui.Application.Professionals.UpdateProfessional;

public sealed class UpdateProfessionalCommandValidator : AbstractValidator<UpdateProfessionalCommand>
{
    public UpdateProfessionalCommandValidator()
    {
        RuleFor(x => x.ProfessionalId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(320);

        RuleFor(x => x.Phone)
            .NotEmpty()
            .MaximumLength(20);
    }
}
