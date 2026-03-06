using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Professionals;
using AgendeAqui.Domain.ValueObjects;

namespace AgendeAqui.Application.Professionals.CreateProfessional;

public sealed class CreateProfessionalCommandHandler(
    IProfessionalRepository professionalRepository,
    IUnitOfWork unitOfWork,
    ITenantProvider tenantProvider) : ICommandHandler<CreateProfessionalCommand, Guid>
{
    public async ValueTask<Result<Guid>> Handle(
        CreateProfessionalCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantProvider.GetTenantId();

        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
            return Result.Failure<Guid>(emailResult.Error);

        var phoneResult = PhoneNumber.Create(command.Phone);
        if (phoneResult.IsFailure)
            return Result.Failure<Guid>(phoneResult.Error);

        var professionalResult = Professional.Create(tenantId, command.Name, emailResult.Value, phoneResult.Value);
        if (professionalResult.IsFailure)
            return Result.Failure<Guid>(professionalResult.Error);

        var professional = professionalResult.Value;

        await professionalRepository.AddAsync(professional, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(professional.Id);
    }
}
