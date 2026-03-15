using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Payments;

namespace AgendeAqui.Application.Payments.UpdatePaymentStatus;

public sealed class UpdatePaymentStatusCommandHandler(
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdatePaymentStatusCommand>
{
    public async ValueTask<Result<Mediator.Unit>> Handle(
        UpdatePaymentStatusCommand command,
        CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetByIdAsync(command.Id, cancellationToken);
        if (payment is null)
            return Result.Failure<Mediator.Unit>(PaymentErrors.NotFound);

        if (!Enum.TryParse<PaymentStatus>(command.Status, ignoreCase: true, out var status))
            return Result.Failure<Mediator.Unit>(PaymentErrors.InvalidStatus);

        var transitionResult = status switch
        {
            PaymentStatus.Paid => payment.MarkAsPaid(),
            PaymentStatus.Refunded => payment.Refund(),
            _ => Result.Failure(PaymentErrors.InvalidStatus),
        };

        if (transitionResult.IsFailure)
            return Result.Failure<Mediator.Unit>(transitionResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Mediator.Unit.Value);
    }
}
