using SpaceDebrisMonitor.Domain.Enums;

namespace SpaceDebrisMonitor.Domain.Entities;

/// <summary>
/// Represents a sensor aboard a satellite.
/// Can be optical, radar, infrared, or LiDAR.
/// </summary>
public class Sensor : BaseEntity
{
    public string ModelName { get; private set; } = string.Empty;
    public SensorType Type { get; private set; }
    public double MinDetectableSizeMeters { get; private set; }
    public double MaxRangeKm { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime LastCalibration { get; private set; }
    public DateTime? NextCalibrationDue { get; private set; }
    public Guid SatelliteId { get; private set; }
    public Satellite Satellite { get; private set; } = null!;

    protected Sensor() { }

    public Sensor(
        string modelName,
        SensorType type,
        double minDetectableSizeMeters,
        double maxRangeKm,
        Guid satelliteId,
        DateTime lastCalibration)
    {
        if (string.IsNullOrWhiteSpace(modelName)) throw new ArgumentException("Nome do modelo é obrigatório.");
        if (minDetectableSizeMeters <= 0) throw new ArgumentOutOfRangeException(nameof(minDetectableSizeMeters));
        if (maxRangeKm <= 0) throw new ArgumentOutOfRangeException(nameof(maxRangeKm));

        ModelName = modelName;
        Type = type;
        MinDetectableSizeMeters = minDetectableSizeMeters;
        MaxRangeKm = maxRangeKm;
        SatelliteId = satelliteId;
        LastCalibration = lastCalibration;
        IsActive = true;
        NextCalibrationDue = lastCalibration.AddMonths(6);
    }

    public void ToggleActive(bool active)
    {
        IsActive = active;
        MarkAsUpdated();
    }

    public void RecordCalibration()
    {
        LastCalibration = DateTime.UtcNow;
        NextCalibrationDue = LastCalibration.AddMonths(6);
        MarkAsUpdated();
    }

    public bool IsCalibrationOverdue() =>
        NextCalibrationDue.HasValue && DateTime.UtcNow > NextCalibrationDue.Value;

    public bool CanDetect(double objectSizeMeters) =>
        IsActive && objectSizeMeters >= MinDetectableSizeMeters;
}
