using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Tenants;

public sealed class Tenant : Entity
{
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public string? ConnectionString { get; private set; }
    public TenantStatus Status { get; private set; }
    public TenantPlan Plan { get; private set; }
    public TenantLabels Labels { get; private set; } = TenantLabels.Default();
    public TenantFeatures Features { get; private set; } = TenantFeatures.Default();

    private Tenant() { }

    public static Tenant Create(string name, string slug, TenantPlan plan = TenantPlan.Free)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        return new Tenant
        {
            Name = name,
            Slug = slug.ToLowerInvariant(),
            Status = TenantStatus.Active,
            Plan = plan
        };
    }

    public void Deactivate()
    {
        Status = TenantStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status = TenantStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangePlan(TenantPlan plan)
    {
        Plan = plan;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateLabels(TenantLabels labels)
    {
        Labels = labels;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateFeatures(TenantFeatures features)
    {
        Features = features;
        UpdatedAt = DateTime.UtcNow;
    }
}
