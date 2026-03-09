using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Tenants.UpdateTenant;

public sealed record UpdateTenantCommand(
    Guid TenantId,
    string Name,
    string Plan) : ICommand;
