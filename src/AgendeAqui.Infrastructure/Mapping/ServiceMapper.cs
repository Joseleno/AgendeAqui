using AgendeAqui.Application.Services.GetService;
using AgendeAqui.Domain.Services;
using Riok.Mapperly.Abstractions;

namespace AgendeAqui.Infrastructure.Mapping;

[Mapper]
internal sealed partial class ServiceMapper
{
    [MapperIgnoreSource(nameof(Service.UpdatedAt))]
    [MapperIgnoreSource(nameof(Service.TenantId))]
    public partial ServiceResponse ToResponse(Service service);

    private static int MapDuration(TimeSpan duration) => (int)duration.TotalMinutes;
}
