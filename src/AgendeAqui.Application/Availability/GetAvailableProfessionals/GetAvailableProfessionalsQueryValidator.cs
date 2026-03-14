using FluentValidation;

namespace AgendeAqui.Application.Availability.GetAvailableProfessionals;

public sealed class GetAvailableProfessionalsQueryValidator : AbstractValidator<GetAvailableProfessionalsQuery>
{
    public GetAvailableProfessionalsQueryValidator()
    {
        RuleFor(x => x.ServiceId).NotEmpty();
        RuleFor(x => x.Date).NotEmpty();
    }
}
