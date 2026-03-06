using AgendeAqui.Application.Professionals.GetProfessional;
using AgendeAqui.Domain.Professionals;
using AgendeAqui.Domain.ValueObjects;
using Riok.Mapperly.Abstractions;

namespace AgendeAqui.Infrastructure.Mapping;

[Mapper]
internal sealed partial class ProfessionalMapper
{
    [MapperIgnoreSource(nameof(Professional.TenantId))]
    [MapperIgnoreSource(nameof(Professional.UpdatedAt))]
    public partial ProfessionalResponse ToResponse(Professional professional);

    private static string MapEmail(Email email) => email.Value;
    private static string MapPhone(PhoneNumber phone) => phone.Value;
}
