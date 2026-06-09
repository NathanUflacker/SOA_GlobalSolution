using SpaceDebrisMonitor.Domain.Enums;

namespace SpaceDebrisMonitor.Application.DTOs;

// ── Request DTOs ──────────────────────────────────────────────────

public record CreateSpaceDebrisRequest(
    string CatalogNumber,
    string Name,
    string? Description,
    DebrisType Type,
    OrbitType OrbitType,
    double SizeMeters,
    double MassKg,
    OrbitalPositionDto InitialPosition,
    string? OriginCountry,
    string? OriginMission,
    DateTime? LaunchDate
);

public record UpdateSpaceDebrisRequest(
    string Name,
    string? Description
);

public record UpdatePositionRequest(
    double Altitude,
    double Inclination,
    double RightAscension,
    double Eccentricity,
    double Velocity
);

// ── Response DTOs (VO — read-only snapshots) ─────────────────────

public record OrbitalPositionDto(
    double Altitude,
    double Inclination,
    double RightAscension,
    double Eccentricity,
    double Velocity,
    DateTime MeasuredAt
);

public record SpaceDebrisSummaryDto(
    Guid Id,
    string CatalogNumber,
    string Name,
    DebrisType Type,
    OrbitType OrbitType,
    double SizeMeters,
    double CollisionProbability,
    bool IsHighRisk,
    OrbitalPositionDto CurrentPosition
);

public record SpaceDebrisDetailDto(
    Guid Id,
    string CatalogNumber,
    string Name,
    string? Description,
    DebrisType Type,
    OrbitType OrbitType,
    double SizeMeters,
    double MassKg,
    string? OriginCountry,
    string? OriginMission,
    DateTime? LaunchDate,
    double CollisionProbability,
    bool IsHighRisk,
    OrbitalPositionDto CurrentPosition,
    IEnumerable<OrbitalPositionDto> PositionHistory,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

// ── Satellite DTOs ────────────────────────────────────────────────

public record CreateSatelliteRequest(
    string Name,
    string NoradId,
    string OperatorOrganization,
    OrbitalPositionDto InitialPosition,
    DateTime LaunchDate,
    double CoverageRadiusKm
);

public record SatelliteSummaryDto(
    Guid Id,
    string Name,
    string NoradId,
    string OperatorOrganization,
    string Status,
    int ActiveSensors,
    double Altitude,
    DateTime LaunchDate
);

// ── Alert DTOs ────────────────────────────────────────────────────

public record CreateAlertRequest(
    Guid SpaceDebrisId,
    Guid? SatelliteId,
    string Title,
    string Message,
    double EstimatedDistanceKm,
    double CollisionProbability,
    DateTime? PredictedClosestApproach
);

public record AlertResponseDto(
    Guid Id,
    AlertSeverity Severity,
    AlertStatus Status,
    string Title,
    string Message,
    double EstimatedDistanceKm,
    double CollisionProbability,
    DateTime? PredictedClosestApproach,
    string DebrisCatalogNumber,
    string? SatelliteName,
    DateTime CreatedAt,
    DateTime? AcknowledgedAt,
    DateTime? ResolvedAt,
    string? ResolutionNotes
);

public record ResolveAlertRequest(string Notes);

// ── Auth DTOs ─────────────────────────────────────────────────────

public record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string? Organization
);

public record LoginRequest(string Username, string Password);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string Username,
    string Role
);

public record RefreshTokenRequest(string RefreshToken);

// ── Dashboard / Analytics DTOs ────────────────────────────────────

public record DashboardStatsDto(
    int TotalDebrisTracked,
    int HighRiskObjects,
    int ActiveAlerts,
    int CriticalAlerts,
    int OperationalSatellites,
    Dictionary<string, int> DebrisByOrbit,
    Dictionary<string, int> DebrisByType
);

// ── Paged Response wrapper ────────────────────────────────────────

public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
