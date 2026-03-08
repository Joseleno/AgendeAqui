using FluentValidation;

namespace AgendeAqui.Application.Clients.DeleteClientData;

public sealed class DeleteClientDataCommandValidator : AbstractValidator<DeleteClientDataCommand>
{
    public DeleteClientDataCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty();
    }
}
