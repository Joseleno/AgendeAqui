using FluentValidation;

namespace AgendeAqui.Application.Clients.ExportClientData;

public sealed class ExportClientDataQueryValidator : AbstractValidator<ExportClientDataQuery>
{
    public ExportClientDataQueryValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty();
    }
}
