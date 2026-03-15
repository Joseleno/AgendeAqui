using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Payments;

public static class PaymentErrors
{
    public static readonly Error NotFound = new("Payment.NotFound", "Payment not found.");
    public static readonly Error InvalidAmount = new("Payment.InvalidAmount", "Payment amount must be greater than zero.");
    public static readonly Error InvalidMethod = new("Payment.InvalidMethod", "Invalid payment method.");
    public static readonly Error InvalidStatus = new("Payment.InvalidStatus", "Invalid payment status.");
    public static readonly Error AppointmentNotFound = new("Payment.AppointmentNotFound", "The specified appointment was not found.");
    public static readonly Error InvalidTransition = new("Payment.InvalidTransition", "The payment status transition is not allowed.");
    public static readonly Error AppointmentNotEligible = new("Payment.AppointmentNotEligible", "Cannot create a payment for a cancelled or no-show appointment.");
}
