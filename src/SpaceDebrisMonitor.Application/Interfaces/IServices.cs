using SpaceDebrisMonitor.Application.DTOs;
using SpaceDebrisMonitor.Domain.Enums;

namespace SpaceDebrisMonitor.Application.Interfaces;

public interface ISpaceDebrisService
{
    Task<PagedResult<SpaceDebrisSummaryDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<SpaceDebrisDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<SpaceDebrisSummaryDto> CreateAsync(CreateSpaceDebrisRequest request, CancellationToken ct = default);
    Task<SpaceDebrisDetailDto?> UpdateAsync(Guid id, UpdateSpaceDebrisRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> UpdatePositionAsync(Guid id, UpdatePositionRequest request, CancellationToken ct = default);
    Task<IEnumerable<SpaceDebrisSummaryDto>> GetHighRiskAsync(CancellationToken ct = default);
    Task<IEnumerable<SpaceDebrisSummaryDto>> GetByOrbitAsync(OrbitType orbit, CancellationToken ct = default);
    Task<IEnumerable<SpaceDebrisSummaryDto>> SearchAsync(string term, CancellationToken ct = default);
}

public interface ISatelliteService
{
    Task<IEnumerable<SatelliteSummaryDto>> GetAllAsync(CancellationToken ct = default);
    Task<SatelliteSummaryDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<SatelliteSummaryDto> CreateAsync(CreateSatelliteRequest request, CancellationToken ct = default);
    Task<bool> UpdateStatusAsync(Guid id, SatelliteStatus status, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface IAlertService
{
    Task<IEnumerable<AlertResponseDto>> GetActiveAlertsAsync(CancellationToken ct = default);
    Task<IEnumerable<AlertResponseDto>> GetByDebrisIdAsync(Guid debrisId, CancellationToken ct = default);
    Task<AlertResponseDto> CreateAlertAsync(CreateAlertRequest request, CancellationToken ct = default);
    Task<bool> AcknowledgeAsync(Guid id, CancellationToken ct = default);
    Task<bool> ResolveAsync(Guid id, ResolveAlertRequest request, CancellationToken ct = default);
    Task<bool> MarkFalsePositiveAsync(Guid id, string reason, CancellationToken ct = default);
    Task<IEnumerable<AlertResponseDto>> GetAlertsInPeriodAsync(DateTime from, DateTime to, CancellationToken ct = default);
}

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default);
    Task<bool> RevokeTokenAsync(string refreshToken, CancellationToken ct = default);
}

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(CancellationToken ct = default);
}

/// <summary>
/// Interface for the AI trajectory prediction service.
/// Decoupled so it can be swapped (real ML model vs stub).
/// </summary>
public interface ITrajectoryPredictionService
{
    Task<IEnumerable<OrbitalPositionDto>> PredictTrajectoryAsync(Guid debrisId, int hoursAhead, CancellationToken ct = default);
    Task<double> EstimateCollisionProbabilityAsync(Guid debrisId, Guid satelliteId, CancellationToken ct = default);
}
