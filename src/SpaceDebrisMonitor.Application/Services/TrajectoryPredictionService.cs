using SpaceDebrisMonitor.Application.DTOs;
using SpaceDebrisMonitor.Application.Interfaces;
using SpaceDebrisMonitor.Domain.Interfaces;

namespace SpaceDebrisMonitor.Application.Services;

/// <summary>
/// Stub AI trajectory prediction service.
/// In production, this would call an ML model or external API.
/// </summary>
public class TrajectoryPredictionService : ITrajectoryPredictionService
{
    private readonly IUnitOfWork _uow;
    public TrajectoryPredictionService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<OrbitalPositionDto>> PredictTrajectoryAsync(Guid debrisId, int hoursAhead, CancellationToken ct = default)
    {
        var debris = await _uow.SpaceDebris.GetByIdAsync(debrisId, ct)
            ?? throw new KeyNotFoundException($"Detrito {debrisId} não encontrado.");

        var predictions = new List<OrbitalPositionDto>();
        var current = debris.CurrentPosition;
        var rng = new Random(debrisId.GetHashCode());

        // Simplified orbital mechanics simulation
        for (int h = 1; h <= hoursAhead; h++)
        {
            // Atmospheric drag causes altitude decay in LEO
            double altitudeDrift = current.Altitude < 600 ? -0.01 * h : 0;
            double raDrift = (current.Velocity / (2 * Math.PI * (6371 + current.Altitude))) * 3600 * h % 360;

            predictions.Add(new OrbitalPositionDto(
                current.Altitude + altitudeDrift + rng.NextDouble() * 0.1 - 0.05,
                current.Inclination + rng.NextDouble() * 0.02 - 0.01,
                (current.RightAscension + raDrift) % 360,
                current.Eccentricity,
                current.Velocity + rng.NextDouble() * 0.001 - 0.0005,
                DateTime.UtcNow.AddHours(h)));
        }
        return predictions;
    }

    public async Task<double> EstimateCollisionProbabilityAsync(Guid debrisId, Guid satelliteId, CancellationToken ct = default)
    {
        var debris = await _uow.SpaceDebris.GetByIdAsync(debrisId, ct)
            ?? throw new KeyNotFoundException("Detrito não encontrado.");
        var satellite = await _uow.Satellites.GetByIdAsync(satelliteId, ct)
            ?? throw new KeyNotFoundException("Satélite não encontrado.");

        var distance = debris.CurrentPosition.DistanceTo(satellite.CurrentPosition);
        // Simplified probability based on distance
        return distance < 1 ? 0.08 : distance < 5 ? 0.02 : distance < 25 ? 0.005 : 0.0001;
    }
}
