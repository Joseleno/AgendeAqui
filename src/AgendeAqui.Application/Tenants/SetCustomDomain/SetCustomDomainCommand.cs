using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Tenants.SetCustomDomain;

public sealed record SetCustomDomainCommand(Guid TenantId, string? Domain) : ICommand;
