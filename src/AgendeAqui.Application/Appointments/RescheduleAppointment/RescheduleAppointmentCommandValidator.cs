using FluentValidation;

namespace AgendeAqui.Application.Appointments.RescheduleAppointment;

public sealed class RescheduleAppointmentCommandValidator : AbstractValidator<RescheduleAppointmentCommand>
{
    public RescheduleAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty();

        RuleFor(x => x.NewDate)
            .NotEmpty()
            .GreaterThan(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("New date must be in the future.");
    }
}
