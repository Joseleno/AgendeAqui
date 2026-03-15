using FluentValidation;

namespace AgendeAqui.Application.Appointments.GenerateMeetingLink;

public sealed class GenerateMeetingLinkCommandValidator : AbstractValidator<GenerateMeetingLinkCommand>
{
    public GenerateMeetingLinkCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
    }
}
