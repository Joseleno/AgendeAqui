using FluentValidation;

namespace AgendeAqui.Application.Appointments.CreateAppointment;

public sealed class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentCommandValidator()
    {
        RuleFor(x => x.ProfessionalId)
            .NotEmpty();

        RuleFor(x => x.ServiceId)
            .NotEmpty();

        RuleFor(x => x.ClientId)
            .NotEmpty();

        RuleFor(x => x.Date)
            .NotEmpty()
            .GreaterThan(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Appointment date must be in the future.");

        RuleFor(x => x.StartTime)
            .NotEqual(default(TimeOnly))
            .WithMessage("A valid start time is required.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000);
    }
}
