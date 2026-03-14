using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;
using AgendeAqui.Domain.Professionals;

namespace AgendeAqui.Application.Professionals.LinkService;

public sealed class LinkServiceToProfessionalCommandHandler(
    IProfessionalRepository professionalRepository,
    IServiceRepository serviceRepository,
    IProfessionalServiceRepository professionalServiceRepository,
    IUnitOfWork unitOfWork,
    ITenantProvider tenantProvider) : ICommandHandler<LinkServiceToProfessionalCommand>
{
    public async ValueTask<Result<Mediator.Unit>> Handle(
        LinkServiceToProfessionalCommand command,
        CancellationToken cancellationToken)
    {
        var professional = await professionalRepository.GetByIdAsync(command.ProfessionalId, cancellationToken);
        if (professional is null)
            return Result.Failure<Mediator.Unit>(ProfessionalErrors.NotFound);

        var service = await serviceRepository.GetByIdAsync(command.ServiceId, cancellationToken);
        if (service is null)
            return Result.Failure<Mediator.Unit>(new Error("Service.NotFound", "Service not found."));

        var exists = await professionalServiceRepository.ExistsAsync(command.ProfessionalId, command.ServiceId, cancellationToken);
        if (exists)
            return Result.Failure<Mediator.Unit>(ProfessionalErrors.ServiceAlreadyLinked);

        var tenantId = tenantProvider.GetTenantId();
        var linkResult = ProfessionalService.Create(tenantId, command.ProfessionalId, command.ServiceId);
        if (linkResult.IsFailure)
            return Result.Failure<Mediator.Unit>(linkResult.Error);

        await professionalServiceRepository.AddAsync(linkResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Mediator.Unit.Value);
    }
}
