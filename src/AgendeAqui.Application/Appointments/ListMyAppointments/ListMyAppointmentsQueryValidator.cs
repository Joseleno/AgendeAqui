using FluentValidation;

namespace AgendeAqui.Application.Appointments.ListMyAppointments;

public sealed class ListMyAppointmentsQueryValidator : AbstractValidator<ListMyAppointmentsQuery>
{
    private static readonly string[] ValidStatuses =
        ["Scheduled", "Confirmed", "InProgress", "Completed", "Cancelled", "NoShow"];

    public ListMyAppointmentsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50)
            .WithMessage("Page size must be between 1 and 50.");

        RuleFor(x => x.Status)
            .Must(s => ValidStatuses.Contains(s))
            .When(x => x.Status is not null)
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}.");
    }
}
