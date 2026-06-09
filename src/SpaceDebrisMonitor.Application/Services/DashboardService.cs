using SpaceDebrisMonitor.Application.DTOs;
using SpaceDebrisMonitor.Application.Interfaces;
using SpaceDebrisMonitor.Domain.Interfaces;

namespace SpaceDebrisMonitor.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _uow;
    public DashboardService(IUnitOfWork uow) => _uow = uow;

    public async Task<DashboardStatsDto> GetStatsAsync(CancellationToken ct = default)
    {
        var allDebris = (await _uow.SpaceDebris.GetAllAsync(ct)).ToList();
        var allAlerts = (await _uow.Alerts.GetActiveAlertsAsync(ct)).ToList();
        var satellites = (await _uow.Satellites.GetOperationalSatellitesAsync(ct)).ToList();
        var highRisk = await _uow.SpaceDebris.GetHighRiskDebrisAsync(ct);

        var byOrbit = allDebris.GroupBy(d => d.OrbitType.ToString())
            .ToDictionary(g => g.Key, g => g.Count());
        var byType = allDebris.GroupBy(d => d.Type.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        return new DashboardStatsDto(
            allDebris.Count,
            highRisk.Count(),
            allAlerts.Count,
            allAlerts.Count(a => a.Severity == Domain.Enums.AlertSeverity.Critical),
            satellites.Count,
            byOrbit,
            byType);
    }
}
