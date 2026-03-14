using FluentValidation;

namespace AgendeAqui.Application.Auth.Register;

public sealed class RegisterPatientCommandValidator : AbstractValidator<RegisterPatientCommand>
{
    public RegisterPatientCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().MaximumLength(320).EmailAddress();
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(128);
    }
}
