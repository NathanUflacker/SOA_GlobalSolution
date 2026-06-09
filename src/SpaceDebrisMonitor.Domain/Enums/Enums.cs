namespace SpaceDebrisMonitor.Domain.Enums;

public enum DebrisType
{
    RocketBody = 1,
    DecommissionedSatellite = 2,
    Fragment = 3,
    Micrometeorite = 4,
    Unknown = 5
}

public enum OrbitType
{
    LEO = 1,    // Low Earth Orbit (200–2000 km)
    MEO = 2,    // Medium Earth Orbit (2000–35786 km)
    GEO = 3,    // Geostationary Orbit (~35786 km)
    HEO = 4,    // Highly Elliptical Orbit
    SSO = 5     // Sun-synchronous Orbit
}

public enum AlertSeverity
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum AlertStatus
{
    Active = 1,
    Acknowledged = 2,
    Resolved = 3,
    FalsePositive = 4
}

public enum SatelliteStatus
{
    Operational = 1,
    Degraded = 2,
    Offline = 3,
    Decommissioned = 4
}

public enum SensorType
{
    Optical = 1,
    Radar = 2,
    Infrared = 3,
    LiDAR = 4
}

public enum UserRole
{
    Viewer = 1,
    Analyst = 2,
    Operator = 3,
    Admin = 4
}
