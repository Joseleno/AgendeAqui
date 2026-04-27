using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Tenants.SuspendTenant;

public sealed record SuspendTenantCommand(Guid TenantId) : ICommand;
