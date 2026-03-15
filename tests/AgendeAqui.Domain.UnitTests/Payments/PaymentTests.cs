using AgendeAqui.Domain.Payments;
using FluentAssertions;

namespace AgendeAqui.Domain.UnitTests.Payments;

public sealed class PaymentTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _appointmentId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldCreatePayment()
    {
        var payment = Payment.Create(_tenantId, _appointmentId, 150.00m, PaymentMethod.Pix, "Test note");

        payment.TenantId.Should().Be(_tenantId);
        payment.AppointmentId.Should().Be(_appointmentId);
        payment.Amount.Should().Be(150.00m);
        payment.Method.Should().Be(PaymentMethod.Pix);
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.Notes.Should().Be("Test note");
        payment.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_WithZeroAmount_ShouldThrow()
    {
        var act = () => Payment.Create(_tenantId, _appointmentId, 0m, PaymentMethod.Cash);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrow()
    {
        var act = () => Payment.Create(_tenantId, _appointmentId, -10m, PaymentMethod.Cash);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void MarkAsPaid_WhenPending_ShouldSucceed()
    {
        var payment = Payment.Create(_tenantId, _appointmentId, 100m, PaymentMethod.Card);

        var result = payment.MarkAsPaid();

        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Paid);
    }

    [Fact]
    public void MarkAsPaid_WhenAlreadyPaid_ShouldReturnFailure()
    {
        var payment = Payment.Create(_tenantId, _appointmentId, 100m, PaymentMethod.Card);
        payment.MarkAsPaid();

        var result = payment.MarkAsPaid();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PaymentErrors.InvalidTransition);
    }

    [Fact]
    public void MarkAsPaid_WhenRefunded_ShouldReturnFailure()
    {
        var payment = Payment.Create(_tenantId, _appointmentId, 100m, PaymentMethod.Card);
        payment.MarkAsPaid();
        payment.Refund();

        var result = payment.MarkAsPaid();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PaymentErrors.InvalidTransition);
    }

    [Fact]
    public void Refund_WhenPaid_ShouldSucceed()
    {
        var payment = Payment.Create(_tenantId, _appointmentId, 100m, PaymentMethod.Pix);
        payment.MarkAsPaid();

        var result = payment.Refund();

        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Refunded);
    }

    [Fact]
    public void Refund_WhenPending_ShouldReturnFailure()
    {
        var payment = Payment.Create(_tenantId, _appointmentId, 100m, PaymentMethod.Pix);

        var result = payment.Refund();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PaymentErrors.InvalidTransition);
    }

    [Fact]
    public void Refund_WhenAlreadyRefunded_ShouldReturnFailure()
    {
        var payment = Payment.Create(_tenantId, _appointmentId, 100m, PaymentMethod.Pix);
        payment.MarkAsPaid();
        payment.Refund();

        var result = payment.Refund();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PaymentErrors.InvalidTransition);
    }
}
