using AgendeAqui.Application.Abstractions.Messaging;

namespace AgendeAqui.Application.Tenants.UpdateTenantTheme;

public sealed record UpdateTenantThemeCommand(
    string PrimaryColor,
    string? LogoUrl,
    string? FaviconUrl) : ICommand;
