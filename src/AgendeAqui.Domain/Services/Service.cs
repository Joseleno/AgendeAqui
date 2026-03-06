using AgendeAqui.Domain.Common;

namespace AgendeAqui.Domain.Services;

public sealed class Service : TenantEntity
{
    public string Name { get; private set; } = default!;
    public TimeSpan Duration { get; private set; }
    public decimal Price { get; private set; }
    public bool IsActive { get; private set; }

    private Service() { }

    public static Result<Service> Create(Guid tenantId, string name, TimeSpan duration, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Service>(ServiceErrors.InvalidName);

        if (duration <= TimeSpan.Zero)
            return Result.Failure<Service>(ServiceErrors.InvalidDuration);

        if (price < 0)
            return Result.Failure<Service>(ServiceErrors.InvalidPrice);

        var service = new Service
        {
            TenantId = tenantId,
            Name = name.Trim(),
            Duration = duration,
            Price = price,
            IsActive = true
        };

        return Result.Success(service);
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, TimeSpan duration, decimal price)
    {
        Name = name.Trim();
        Duration = duration;
        Price = price;
        UpdatedAt = DateTime.UtcNow;
    }
}
