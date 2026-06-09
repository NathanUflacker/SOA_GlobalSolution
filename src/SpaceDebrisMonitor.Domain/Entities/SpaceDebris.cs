using SpaceDebrisMonitor.Domain.Enums;
using SpaceDebrisMonitor.Domain.ValueObjects;

namespace SpaceDebrisMonitor.Domain.Entities;

/// <summary>
/// Represents a tracked space debris object.
/// Central aggregate root of the monitoring domain.
/// </summary>
public class SpaceDebris : BaseEntity
{
    public string CatalogNumber { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DebrisType Type { get; private set; }
    public OrbitType OrbitType { get; private set; }
    public double SizeMeters { get; private set; }       // estimated diameter in meters
    public double MassKg { get; private set; }           // estimated mass
    public string? OriginCountry { get; private set; }
    public string? OriginMission { get; private set; }
    public DateTime? LaunchDate { get; private set; }
    public double CollisionProbability { get; private set; } // 0.0 – 1.0

    public OrbitalPosition CurrentPosition { get; private set; }
    private readonly List<OrbitalPosition> _positionHistory = new();
    public IReadOnlyList<OrbitalPosition> PositionHistory => _positionHistory.AsReadOnly();
    public ICollection<Alert> Alerts { get; private set; } = new List<Alert>();

    // Required by EF Core
    protected SpaceDebris() { CurrentPosition = null!; }

    public SpaceDebris(
        string catalogNumber,
        string name,
        DebrisType type,
        OrbitType orbitType,
        double sizeMeters,
        double massKg,
        OrbitalPosition initialPosition,
        string? originCountry = null,
        string? originMission = null,
        DateTime? launchDate = null)
    {
        if (string.IsNullOrWhiteSpace(catalogNumber)) throw new ArgumentException("Número de catálogo é obrigatório.", nameof(catalogNumber));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Nome é obrigatório.", nameof(name));
        if (sizeMeters <= 0) throw new ArgumentOutOfRangeException(nameof(sizeMeters));
        if (massKg < 0) throw new ArgumentOutOfRangeException(nameof(massKg));

        CatalogNumber = catalogNumber;
        Name = name;
        Type = type;
        OrbitType = orbitType;
        SizeMeters = sizeMeters;
        MassKg = massKg;
        CurrentPosition = initialPosition ?? throw new ArgumentNullException(nameof(initialPosition));
        OriginCountry = originCountry;
        OriginMission = originMission;
        LaunchDate = launchDate;
        CollisionProbability = 0.0;
        _positionHistory.Add(initialPosition);
    }

    /// <summary>
    /// Updates the object's orbital position and logs it in history.
    /// </summary>
    public void UpdatePosition(OrbitalPosition newPosition)
    {
        ArgumentNullException.ThrowIfNull(newPosition);
        _positionHistory.Add(CurrentPosition);
        CurrentPosition = newPosition;
        MarkAsUpdated();
    }

    /// <summary>
    /// Recalculates the collision probability based on proximity to a reference position.
    /// Uses an inverse-square law relative to a 10km danger threshold.
    /// </summary>
    public void RecalculateCollisionProbability(OrbitalPosition reference)
    {
        const double DangerThresholdKm = 10.0;
        double distance = CurrentPosition.DistanceTo(reference);
        CollisionProbability = distance < DangerThresholdKm
            ? Math.Min(1.0, Math.Pow(DangerThresholdKm / Math.Max(distance, 0.01), 2) * 0.01)
            : 0.0;
        MarkAsUpdated();
    }

    public void UpdateDetails(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Nome não pode ficar vazio.");
        Name = name;
        Description = description;
        MarkAsUpdated();
    }

    public bool IsHighRisk() => CollisionProbability >= 0.01;
}
