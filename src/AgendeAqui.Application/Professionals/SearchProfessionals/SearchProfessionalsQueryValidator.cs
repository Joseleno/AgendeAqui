using FluentValidation;

namespace AgendeAqui.Application.Professionals.SearchProfessionals;

public sealed class SearchProfessionalsQueryValidator : AbstractValidator<SearchProfessionalsQuery>
{
    public SearchProfessionalsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50)
            .WithMessage("PageSize must be between 1 and 50.");
    }
}
