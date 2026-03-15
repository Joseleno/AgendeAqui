using FluentValidation;

namespace AgendeAqui.Application.Payments.CreatePayment;

public sealed class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).LessThanOrEqualTo(999_999.99m);
        RuleFor(x => x.Method)
            .NotEmpty()
            .Must(m => m is "Pix" or "Cash" or "Card" or "Transfer")
            .WithMessage("Método de pagamento inválido.");
    }
}
