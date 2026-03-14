using FluentValidation;

namespace AgendeAqui.Application.Appointments.GetMyFilledSlots;

public sealed class GetMyFilledSlotsQueryValidator : AbstractValidator<GetMyFilledSlotsQuery>
{
    public GetMyFilledSlotsQueryValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("'Date' is required.");
    }
}
