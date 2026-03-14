using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Professionals;

public sealed class ProfessionalService : TenantEntity
{
    public Guid ProfessionalId { get; private set; }
    public Guid ServiceId { get; private set; }

    private ProfessionalService() { }

    public static Result<ProfessionalService> Create(Guid tenantId, Guid professionalId, Guid serviceId)
    {
        if (professionalId == Guid.Empty)
            return Result.Failure<ProfessionalService>(ProfessionalErrors.NotFound);

        if (serviceId == Guid.Empty)
            return Result.Failure<ProfessionalService>(new Error("Service.NotFound", "Service not found."));

        var link = new ProfessionalService
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            TenantId = tenantId,
            ProfessionalId = professionalId,
            ServiceId = serviceId
        };

        return Result.Success(link);
    }
}
