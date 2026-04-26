using FluentValidation;

namespace AgendeAqui.Application.Import.BulkImportProfessionals;

internal sealed class BulkImportProfessionalsCommandValidator : AbstractValidator<BulkImportProfessionalsCommand>
{
    private const int MaxItems = 500;

    public BulkImportProfessionalsCommandValidator()
    {
        RuleFor(x => x.Professionals)
            .NotNull()
            .Must(p => p.Count <= MaxItems)
            .WithMessage($"Cannot import more than {MaxItems} professionals at once.");

        RuleForEach(x => x.Professionals).ChildRules(item =>
        {
            item.RuleFor(i => i.Name).NotEmpty().MaximumLength(200);
            item.RuleFor(i => i.Email).NotEmpty().EmailAddress().MaximumLength(320);
            item.RuleFor(i => i.Phone).NotEmpty().MaximumLength(20);
            item.RuleFor(i => i.Specialty).MaximumLength(100).When(i => i.Specialty is not null);
        });
    }
}
