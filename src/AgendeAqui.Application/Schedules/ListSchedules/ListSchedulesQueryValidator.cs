using FluentValidation;

namespace AgendeAqui.Application.Schedules.ListSchedules;

public sealed class ListSchedulesQueryValidator : AbstractValidator<ListSchedulesQuery>
{
    public ListSchedulesQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50)
            .WithMessage("Page size must be between 1 and 50.");
    }
}
