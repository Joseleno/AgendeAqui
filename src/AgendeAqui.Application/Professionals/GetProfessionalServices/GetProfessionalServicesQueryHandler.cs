using AgendeAqui.Application.Abstractions.Messaging;
using AgendeAqui.Domain.Abstractions;
using AgendeAqui.Domain.Common;

namespace AgendeAqui.Application.Professionals.GetProfessionalServices;

public sealed class GetProfessionalServicesQueryHandler(
    IProfessionalServiceRepository professionalServiceRepository) : IQueryHandler<GetProfessionalServicesQuery, List<Guid>>
{
    public async ValueTask<Result<List<Guid>>> Handle(
        GetProfessionalServicesQuery query,
        CancellationToken cancellationToken)
    {
        var serviceIds = await professionalServiceRepository.GetServiceIdsByProfessionalAsync(
            query.ProfessionalId, cancellationToken);

        return Result.Success(serviceIds.ToList());
    }
}
