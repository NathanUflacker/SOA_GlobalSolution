using SpaceDebrisMonitor.Application.DTOs;
using SpaceDebrisMonitor.Application.Interfaces;
using SpaceDebrisMonitor.Domain.Entities;
using SpaceDebrisMonitor.Domain.Enums;
using SpaceDebrisMonitor.Domain.Interfaces;
using SpaceDebrisMonitor.Domain.ValueObjects;

namespace SpaceDebrisMonitor.Application.Services;

public class SatelliteService : ISatelliteService
{
    private readonly IUnitOfWork _uow;
    public SatelliteService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<SatelliteSummaryDto>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _uow.Satellites.GetAllAsync(ct);
        return items.Select(Map);
    }

    public async Task<SatelliteSummaryDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var s = await _uow.Satellites.GetByIdAsync(id, ct);
        return s is null ? null : Map(s);
    }

    public async Task<SatelliteSummaryDto> CreateAsync(CreateSatelliteRequest request, CancellationToken ct = default)
    {
        var pos = new OrbitalPosition(
            request.InitialPosition.Altitude, request.InitialPosition.Inclination,
            request.InitialPosition.RightAscension, request.InitialPosition.Eccentricity,
            request.InitialPosition.Velocity, DateTime.UtcNow);
        var sat = new Satellite(request.Name, request.NoradId, request.OperatorOrganization,
            pos, request.LaunchDate, request.CoverageRadiusKm);
        await _uow.Satellites.AddAsync(sat, ct);
        await _uow.SaveChangesAsync(ct);
        return Map(sat);
    }

    public async Task<bool> UpdateStatusAsync(Guid id, SatelliteStatus status, CancellationToken ct = default)
    {
        var sat = await _uow.Satellites.GetByIdAsync(id, ct);
        if (sat is null) return false;
        sat.UpdateStatus(status);
        await _uow.Satellites.UpdateAsync(sat, ct);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var sat = await _uow.Satellites.GetByIdAsync(id, ct);
        if (sat is null) return false;
        await _uow.Satellites.DeleteAsync(id, ct);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    private static SatelliteSummaryDto Map(Satellite s) => new(
        s.Id, s.Name, s.NoradId, s.OperatorOrganization, s.Status.ToString(),
        s.Sensors.Count(sen => sen.IsActive), s.CurrentPosition.Altitude, s.LaunchDate);
}
