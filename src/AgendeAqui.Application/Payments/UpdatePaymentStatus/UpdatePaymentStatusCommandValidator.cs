using FluentValidation;

namespace AgendeAqui.Application.Payments.UpdatePaymentStatus;

public sealed class UpdatePaymentStatusCommandValidator : AbstractValidator<UpdatePaymentStatusCommand>
{
    public UpdatePaymentStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => s is "Paid" or "Refunded")
            .WithMessage("Status inválido. Valores aceitos: Paid, Refunded.");
    }
}
