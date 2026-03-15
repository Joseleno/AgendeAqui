using FluentValidation;

namespace AgendeAqui.Application.Payments.GetPaymentSummary;

public sealed class GetPaymentSummaryQueryValidator : AbstractValidator<GetPaymentSummaryQuery>
{
    public GetPaymentSummaryQueryValidator()
    {
        RuleFor(x => x.From)
            .LessThanOrEqualTo(x => x.To)
            .WithMessage("'From' must be less than or equal to 'To'.");
    }
}
