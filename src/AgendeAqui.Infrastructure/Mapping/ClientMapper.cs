using AgendeAqui.Application.Clients.GetClient;
using AgendeAqui.Domain.Clients;
using AgendeAqui.Domain.ValueObjects;
using Riok.Mapperly.Abstractions;

namespace AgendeAqui.Infrastructure.Mapping;

[Mapper]
internal sealed partial class ClientMapper
{
    [MapperIgnoreSource(nameof(Client.TenantId))]
    [MapperIgnoreSource(nameof(Client.UpdatedAt))]
    public partial ClientResponse ToResponse(Client client);

    private static string MapEmail(Email email) => email.Value;
    private static string MapPhone(PhoneNumber phone) => phone.Value;
}
