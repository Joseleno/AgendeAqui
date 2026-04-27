namespace AgendeAqui.Domain.Tenants;

public sealed record TenantTheme
{
    public string PrimaryColor { get; init; } = "#6366f1";
    public string? LogoUrl { get; init; }
    public string? FaviconUrl { get; init; }
    public string? CustomCss { get; init; }

    public static TenantTheme Default() => new();

    public TenantTheme WithPrimaryColor(string color) => this with { PrimaryColor = color };
    public TenantTheme WithLogoUrl(string? url) => this with { LogoUrl = url };
    public TenantTheme WithFaviconUrl(string? url) => this with { FaviconUrl = url };
}
