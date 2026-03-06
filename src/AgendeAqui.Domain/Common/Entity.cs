namespace AgendeAqui.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }

    public override bool Equals(object? obj) =>
        obj is Entity other && Id == other.Id;

    public override int GetHashCode() => Id.GetHashCode();
}
