using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Tenants.ActivateTenant;

public sealed record ActivateTenantCommand(Guid TenantId) : ICommand;
