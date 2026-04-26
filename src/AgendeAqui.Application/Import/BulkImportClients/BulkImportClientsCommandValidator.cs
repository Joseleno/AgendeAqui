using FluentValidation;

namespace AgendeAqui.Application.Import.BulkImportClients;

internal sealed class BulkImportClientsCommandValidator : AbstractValidator<BulkImportClientsCommand>
{
    private const int MaxItems = 500;
    private const int MaxNotesLength = 2000;

    public BulkImportClientsCommandValidator()
    {
        RuleFor(x => x.Clients)
            .NotNull()
            .Must(c => c.Count <= MaxItems)
            .WithMessage($"Cannot import more than {MaxItems} clients at once.");

        RuleForEach(x => x.Clients).ChildRules(item =>
        {
            item.RuleFor(i => i.Name).NotEmpty().MaximumLength(200);
            item.RuleFor(i => i.Email).NotEmpty().EmailAddress().MaximumLength(320);
            item.RuleFor(i => i.Phone).NotEmpty().MaximumLength(20);
            item.RuleFor(i => i.Notes)
                .MaximumLength(MaxNotesLength)
                .When(i => i.Notes is not null);
        });
    }
}
