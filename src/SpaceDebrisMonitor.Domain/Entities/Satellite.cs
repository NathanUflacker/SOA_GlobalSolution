using SpaceDebrisMonitor.Domain.Enums;
using SpaceDebrisMonitor.Domain.ValueObjects;

namespace SpaceDebrisMonitor.Domain.Entities;

/// <summary>
/// Abstract base for any object actively monitored with sensors.
/// Demonstrates abstract class and polymorphism.
/// </summary>
public abstract class MonitoredObject : BaseEntity
{
    public string Name { get; protected set; } = string.Empty;
    public string OperatorOrganization { get; protected set; } = string.Empty;
    public OrbitalPosition CurrentPosition { get; protected set; } = null!;

    public abstract string GetObjectType();
    public abstract string GetStatusSummary();

    public virtual double GetRiskScore() => 0.0;
}

/// <summary>
/// Represents an active monitoring satellite with sensor capabilities.
/// Inherits from MonitoredObject and demonstrates polymorphism.
/// </summary>
public class Satellite : MonitoredObject
{
    public string NoradId { get; private set; } = string.Empty;
    public SatelliteStatus Status { get; private set; }
    public DateTime LaunchDate { get; private set; }
    public DateTime? DecommissionDate { get; private set; }
    public double CoverageRadiusKm { get; private set; }
    public ICollection<Sensor> Sensors { get; private set; } = new List<Sensor>();
    public ICollection<Alert> GeneratedAlerts { get; private set; } = new List<Alert>();

    protected Satellite() { }

    public Satellite(
        string name,
        string noradId,
        string operatorOrganization,
        OrbitalPosition initialPosition,
        DateTime launchDate,
        double coverageRadiusKm = 500.0)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Nome é obrigatório.");
        if (string.IsNullOrWhiteSpace(noradId)) throw new ArgumentException("ID NORAD é obrigatório.");

        Name = name;
        NoradId = noradId;
        OperatorOrganization = operatorOrganization;
        CurrentPosition = initialPosition ?? throw new ArgumentNullException(nameof(initialPosition));
        LaunchDate = launchDate;
        Status = SatelliteStatus.Operational;
        CoverageRadiusKm = coverageRadiusKm;
    }

    public override string GetObjectType() => "Satélite de Monitoramento";

    public override string GetStatusSummary() =>
        $"{Name} [{NoradId}] — {Status} | Órbita: {CurrentPosition.Altitude:F0}km | Sensores: {Sensors.Count}";

    public override double GetRiskScore()
    {
        // Satellites in LEO face higher debris risk
        return CurrentPosition.Altitude < 2000 ? 0.35 : 0.10;
    }

    public void UpdateStatus(SatelliteStatus newStatus)
    {
        Status = newStatus;
        if (newStatus == SatelliteStatus.Decommissioned)
            DecommissionDate = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void UpdatePosition(OrbitalPosition position)
    {
        CurrentPosition = position ?? throw new ArgumentNullException(nameof(position));
        MarkAsUpdated();
    }

    public bool IsOperational() => Status == SatelliteStatus.Operational;
}
