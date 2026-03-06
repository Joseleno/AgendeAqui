using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Tenants.CreateTenant;

public sealed record CreateTenantCommand(
    string Name,
    string Slug,
    string Plan) : ICommand<Guid>;
