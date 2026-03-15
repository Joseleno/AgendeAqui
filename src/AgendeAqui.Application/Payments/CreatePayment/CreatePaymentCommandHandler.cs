using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Payments;
using AgendeAqui.Domain.ValueObjects;

namespace AgendeAqui.Application.Payments.CreatePayment;

public sealed class CreatePaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IAppointmentRepository appointmentRepository,
    IUnitOfWork unitOfWork,
    ITenantProvider tenantProvider) : ICommandHandler<CreatePaymentCommand, Guid>
{
    public async ValueTask<Result<Guid>> Handle(
        CreatePaymentCommand command,
        CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.GetByIdAsync(command.AppointmentId, cancellationToken);
        if (appointment is null)
            return Result.Failure<Guid>(PaymentErrors.AppointmentNotFound);

        if (appointment.Status == AppointmentStatus.Cancelled || appointment.Status == AppointmentStatus.NoShow)
            return Result.Failure<Guid>(PaymentErrors.AppointmentNotEligible);

        if (!Enum.TryParse<PaymentMethod>(command.Method, ignoreCase: true, out var method))
            return Result.Failure<Guid>(PaymentErrors.InvalidMethod);

        var tenantId = tenantProvider.GetTenantId();

        var payment = Payment.Create(
            tenantId,
            command.AppointmentId,
            command.Amount,
            method,
            command.Notes);

        await paymentRepository.AddAsync(payment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(payment.Id);
    }
}
