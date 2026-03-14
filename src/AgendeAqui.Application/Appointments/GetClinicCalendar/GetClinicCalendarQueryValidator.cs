using FluentValidation;

namespace AgendeAqui.Application.Appointments.GetClinicCalendar;

public sealed class GetClinicCalendarQueryValidator : AbstractValidator<GetClinicCalendarQuery>
{
    public GetClinicCalendarQueryValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("'Date' is required.");
    }
}
