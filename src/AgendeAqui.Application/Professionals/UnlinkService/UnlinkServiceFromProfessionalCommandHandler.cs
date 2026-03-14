using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Professionals;

namespace AgendeAqui.Application.Professionals.UnlinkService;

public sealed class UnlinkServiceFromProfessionalCommandHandler(
    IProfessionalServiceRepository professionalServiceRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UnlinkServiceFromProfessionalCommand>
{
    public async ValueTask<Result<Mediator.Unit>> Handle(
        UnlinkServiceFromProfessionalCommand command,
        CancellationToken cancellationToken)
    {
        var link = await professionalServiceRepository.GetAsync(command.ProfessionalId, command.ServiceId, cancellationToken);
        if (link is null)
            return Result.Failure<Mediator.Unit>(ProfessionalErrors.ServiceNotLinked);

        professionalServiceRepository.Remove(link);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Mediator.Unit.Value);
    }
}
