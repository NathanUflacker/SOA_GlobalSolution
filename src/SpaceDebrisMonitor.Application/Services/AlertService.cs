using SpaceDebrisMonitor.Application.DTOs;
using SpaceDebrisMonitor.Application.Interfaces;
using SpaceDebrisMonitor.Domain.Entities;
using SpaceDebrisMonitor.Domain.Enums;
using SpaceDebrisMonitor.Domain.Interfaces;

namespace SpaceDebrisMonitor.Application.Services;

public class AlertService : IAlertService
{
    private readonly IUnitOfWork _uow;
    public AlertService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<AlertResponseDto>> GetActiveAlertsAsync(CancellationToken ct = default)
    {
        var alerts = await _uow.Alerts.GetActiveAlertsAsync(ct);
        return alerts.Select(Map);
    }

    public async Task<IEnumerable<AlertResponseDto>> GetByDebrisIdAsync(Guid debrisId, CancellationToken ct = default)
    {
        var alerts = await _uow.Alerts.GetByDebrisIdAsync(debrisId, ct);
        return alerts.Select(Map);
    }

    public async Task<AlertResponseDto> CreateAlertAsync(CreateAlertRequest request, CancellationToken ct = default)
    {
        var severity = Alert.DetermineSeverity(request.EstimatedDistanceKm, request.CollisionProbability);
        var alert = new Alert(request.SpaceDebrisId, severity, request.Title, request.Message,
            request.EstimatedDistanceKm, request.CollisionProbability,
            request.SatelliteId, request.PredictedClosestApproach);
        await _uow.Alerts.AddAsync(alert, ct);
        await _uow.SaveChangesAsync(ct);
        return Map(alert);
    }

    public async Task<bool> AcknowledgeAsync(Guid id, CancellationToken ct = default)
    {
        var alert = await _uow.Alerts.GetByIdAsync(id, ct);
        if (alert is null) return false;
        alert.Acknowledge();
        await _uow.Alerts.UpdateAsync(alert, ct);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ResolveAsync(Guid id, ResolveAlertRequest request, CancellationToken ct = default)
    {
        var alert = await _uow.Alerts.GetByIdAsync(id, ct);
        if (alert is null) return false;
        alert.Resolve(request.Notes);
        await _uow.Alerts.UpdateAsync(alert, ct);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> MarkFalsePositiveAsync(Guid id, string reason, CancellationToken ct = default)
    {
        var alert = await _uow.Alerts.GetByIdAsync(id, ct);
        if (alert is null) return false;
        alert.MarkAsFalsePositive(reason);
        await _uow.Alerts.UpdateAsync(alert, ct);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IEnumerable<AlertResponseDto>> GetAlertsInPeriodAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var alerts = await _uow.Alerts.GetAlertsInPeriodAsync(from, to, ct);
        return alerts.Select(Map);
    }

    private static AlertResponseDto Map(Alert a) => new(
        a.Id, a.Severity, a.Status, a.Title, a.Message,
        a.EstimatedDistanceKm, a.CollisionProbability, a.PredictedClosestApproach,
        a.SpaceDebris?.CatalogNumber ?? a.SpaceDebrisId.ToString(),
        a.Satellite?.Name, a.CreatedAt, a.AcknowledgedAt, a.ResolvedAt, a.ResolutionNotes);
}
