using FluentValidation;

namespace AgendeAqui.Application.Appointments.UpdateAttendance;

public sealed class UpdateAttendanceCommandValidator : AbstractValidator<UpdateAttendanceCommand>
{
    public UpdateAttendanceCommandValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty();

        RuleFor(x => x.Action)
            .IsInEnum()
            .WithMessage("Action must be one of: Confirm, Start, Complete, NoShow.");
    }
}
