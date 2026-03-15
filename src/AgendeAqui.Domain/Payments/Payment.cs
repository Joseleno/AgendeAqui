using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Payments;

public sealed class Payment : TenantEntity
{
    public Guid AppointmentId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentMethod Method { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? Notes { get; private set; }

    private Payment() { }

    public static Payment Create(
        Guid tenantId,
        Guid appointmentId,
        decimal amount,
        PaymentMethod method,
        string? notes = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        return new Payment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AppointmentId = appointmentId,
            Amount = amount,
            Method = method,
            Status = PaymentStatus.Pending,
            Notes = notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public Result MarkAsPaid()
    {
        if (Status != PaymentStatus.Pending)
            return Result.Failure(PaymentErrors.InvalidTransition);

        Status = PaymentStatus.Paid;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Refund()
    {
        if (Status != PaymentStatus.Paid)
            return Result.Failure(PaymentErrors.InvalidTransition);

        Status = PaymentStatus.Refunded;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
