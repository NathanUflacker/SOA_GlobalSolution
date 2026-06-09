namespace SpaceDebrisMonitor.Domain.Entities;

/// <summary>
/// Base entity with audit fields. All domain entities inherit from this.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;
    public bool IsDeleted { get; protected set; } = false;

    protected void MarkAsUpdated() => UpdatedAt = DateTime.UtcNow;
    public void SoftDelete()
    {
        IsDeleted = true;
        MarkAsUpdated();
    }
}
