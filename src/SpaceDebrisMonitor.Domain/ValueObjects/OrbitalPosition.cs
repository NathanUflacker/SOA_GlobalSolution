namespace SpaceDebrisMonitor.Domain.ValueObjects;

/// <summary>
/// Value Object representing the precise orbital position of an object.
/// Immutable by design — equality is based on values, not identity.
/// </summary>
public sealed class OrbitalPosition : IEquatable<OrbitalPosition>
{
    public double Altitude { get; }          // km above sea level
    public double Inclination { get; }       // degrees (0–180)
    public double RightAscension { get; }    // degrees (0–360)
    public double Eccentricity { get; }      // 0 (circular) to <1 (elliptical)
    public double Velocity { get; }          // km/s
    public DateTime MeasuredAt { get; }

    public OrbitalPosition(
        double altitude,
        double inclination,
        double rightAscension,
        double eccentricity,
        double velocity,
        DateTime measuredAt)
    {
        if (altitude < 0) throw new ArgumentOutOfRangeException(nameof(altitude), "Altitude não pode ser negativa.");
        if (inclination < 0 || inclination > 180) throw new ArgumentOutOfRangeException(nameof(inclination));
        if (rightAscension < 0 || rightAscension > 360) throw new ArgumentOutOfRangeException(nameof(rightAscension));
        if (eccentricity < 0 || eccentricity >= 1) throw new ArgumentOutOfRangeException(nameof(eccentricity));
        if (velocity <= 0) throw new ArgumentOutOfRangeException(nameof(velocity), "Velocidade deve ser positiva.");

        Altitude = altitude;
        Inclination = inclination;
        RightAscension = rightAscension;
        Eccentricity = eccentricity;
        Velocity = velocity;
        MeasuredAt = measuredAt;
    }

    /// <summary>
    /// Calculates approximate distance to another orbital position in km.
    /// Uses a simplified 3D Euclidean approximation.
    /// </summary>
    public double DistanceTo(OrbitalPosition other)
    {
        const double EarthRadius = 6371.0; // km
        double r1 = EarthRadius + Altitude;
        double r2 = EarthRadius + other.Altitude;
        double angle = Math.Acos(
            Math.Sin(ToRad(Inclination)) * Math.Sin(ToRad(other.Inclination)) +
            Math.Cos(ToRad(Inclination)) * Math.Cos(ToRad(other.Inclination)) *
            Math.Cos(ToRad(RightAscension - other.RightAscension)));
        return Math.Sqrt(r1 * r1 + r2 * r2 - 2 * r1 * r2 * Math.Cos(angle));
    }

    private static double ToRad(double degrees) => degrees * Math.PI / 180.0;

    public bool Equals(OrbitalPosition? other)
    {
        if (other is null) return false;
        return Altitude == other.Altitude &&
               Inclination == other.Inclination &&
               RightAscension == other.RightAscension &&
               Eccentricity == other.Eccentricity &&
               Velocity == other.Velocity;
    }

    public override bool Equals(object? obj) => Equals(obj as OrbitalPosition);
    public override int GetHashCode() =>
        HashCode.Combine(Altitude, Inclination, RightAscension, Eccentricity, Velocity);
    public override string ToString() =>
        $"Alt:{Altitude:F1}km Inc:{Inclination:F1}° RA:{RightAscension:F1}° Ecc:{Eccentricity:F4} V:{Velocity:F2}km/s";
}
