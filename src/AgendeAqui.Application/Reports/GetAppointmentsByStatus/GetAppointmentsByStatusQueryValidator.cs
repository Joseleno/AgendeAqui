using FluentValidation;

namespace AgendeAqui.Application.Reports.GetAppointmentsByStatus;

public sealed class GetAppointmentsByStatusQueryValidator : AbstractValidator<GetAppointmentsByStatusQuery>
{
    public GetAppointmentsByStatusQueryValidator()
    {
        RuleFor(x => x.From)
            .LessThanOrEqualTo(x => x.To)
            .WithMessage("'From' must be less than or equal to 'To'.");
    }
}
