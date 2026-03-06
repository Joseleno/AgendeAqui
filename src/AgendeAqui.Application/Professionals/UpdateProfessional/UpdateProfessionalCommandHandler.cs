using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Professionals;
using AgendeAqui.Domain.ValueObjects;

namespace AgendeAqui.Application.Professionals.UpdateProfessional;

public sealed class UpdateProfessionalCommandHandler(
    IProfessionalRepository professionalRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateProfessionalCommand>
{
    public async ValueTask<Result<Mediator.Unit>> Handle(
        UpdateProfessionalCommand command,
        CancellationToken cancellationToken)
    {
        var professional = await professionalRepository.GetByIdAsync(command.ProfessionalId, cancellationToken);
        if (professional is null)
            return Result.Failure<Mediator.Unit>(ProfessionalErrors.NotFound);

        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
            return Result.Failure<Mediator.Unit>(emailResult.Error);

        var phoneResult = PhoneNumber.Create(command.Phone);
        if (phoneResult.IsFailure)
            return Result.Failure<Mediator.Unit>(phoneResult.Error);

        professional.UpdateContact(command.Name, emailResult.Value, phoneResult.Value);

        professionalRepository.Update(professional);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Mediator.Unit.Value);
    }
}
