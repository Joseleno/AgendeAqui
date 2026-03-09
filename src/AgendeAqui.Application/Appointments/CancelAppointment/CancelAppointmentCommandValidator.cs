using FluentValidation;

namespace AgendeAqui.Application.Appointments.CancelAppointment;

public sealed class CancelAppointmentCommandValidator : AbstractValidator<CancelAppointmentCommand>
{
    public CancelAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty();

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500);
    }
}
