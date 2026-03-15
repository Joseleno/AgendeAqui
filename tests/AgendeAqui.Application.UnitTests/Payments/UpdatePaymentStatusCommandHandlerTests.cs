using AgendeAqui.Application.Payments.UpdatePaymentStatus;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Payments;
using FluentAssertions;
using NSubstitute;

namespace AgendeAqui.Application.UnitTests.Payments;

public sealed class UpdatePaymentStatusCommandHandlerTests
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UpdatePaymentStatusCommandHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public UpdatePaymentStatusCommandHandlerTests()
    {
        _paymentRepository = Substitute.For<IPaymentRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        _handler = new UpdatePaymentStatusCommandHandler(
            _paymentRepository,
            _unitOfWork);
    }

    private Payment CreatePendingPayment()
    {
        return Payment.Create(_tenantId, Guid.NewGuid(), 100m, PaymentMethod.Pix);
    }

    [Fact]
    public async Task Handle_ValidTransitionToPaid_ShouldSucceed()
    {
        var payment = CreatePendingPayment();
        _paymentRepository.GetByIdAsync(payment.Id, Arg.Any<CancellationToken>()).Returns(payment);

        var command = new UpdatePaymentStatusCommand(payment.Id, "Paid");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Paid);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PaymentNotFound_ShouldReturnError()
    {
        _paymentRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Payment?)null);

        var command = new UpdatePaymentStatusCommand(Guid.NewGuid(), "Paid");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PaymentErrors.NotFound);
    }

    [Fact]
    public async Task Handle_InvalidTransition_ShouldReturnError()
    {
        var payment = CreatePendingPayment();
        _paymentRepository.GetByIdAsync(payment.Id, Arg.Any<CancellationToken>()).Returns(payment);

        // Pending -> Refunded is invalid (must be Paid first)
        var command = new UpdatePaymentStatusCommand(payment.Id, "Refunded");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PaymentErrors.InvalidTransition);
    }
}
