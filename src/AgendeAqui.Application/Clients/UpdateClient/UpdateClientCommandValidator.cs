using FluentValidation;

namespace AgendeAqui.Application.Clients.UpdateClient;

public sealed class UpdateClientCommandValidator : AbstractValidator<UpdateClientCommand>
{
    public UpdateClientCommandValidator()
    {
        RuleFor(x => x.ClientId)
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
