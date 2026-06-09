using SpaceDebrisMonitor.Application.DTOs;
using SpaceDebrisMonitor.Application.Interfaces;
using SpaceDebrisMonitor.Domain.Entities;
using SpaceDebrisMonitor.Domain.Enums;
using SpaceDebrisMonitor.Domain.Interfaces;
using SpaceDebrisMonitor.Domain.ValueObjects;

namespace SpaceDebrisMonitor.Application.Services;

public class SpaceDebrisService : ISpaceDebrisService
{
    private readonly IUnitOfWork _uow;

    public SpaceDebrisService(IUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<SpaceDebrisSummaryDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var all = (await _uow.SpaceDebris.GetAllAsync(ct)).ToList();
        var total = all.Count;
        var items = all
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToSummary);
        return new PagedResult<SpaceDebrisSummaryDto>(items, total, page, pageSize,
            (int)Math.Ceiling((double)total / pageSize));
    }

    public async Task<SpaceDebrisDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _uow.SpaceDebris.GetByIdAsync(id, ct);
        return entity is null ? null : MapToDetail(entity);
    }

    public async Task<SpaceDebrisSummaryDto> CreateAsync(CreateSpaceDebrisRequest request, CancellationToken ct = default)
    {
        var existing = await _uow.SpaceDebris.GetByCatalogNumberAsync(request.CatalogNumber, ct);
        if (existing is not null)
            throw new InvalidOperationException($"Detrito com número de catálogo '{request.CatalogNumber}' já existe.");

        var position = MapToOrbitalPosition(request.InitialPosition);
        var debris = new SpaceDebris(
            request.CatalogNumber, request.Name, request.Type, request.OrbitType,
            request.SizeMeters, request.MassKg, position,
            request.OriginCountry, request.OriginMission, request.LaunchDate);

        await _uow.SpaceDebris.AddAsync(debris, ct);
        await _uow.SaveChangesAsync(ct);
        return MapToSummary(debris);
    }

    public async Task<SpaceDebrisDetailDto?> UpdateAsync(Guid id, UpdateSpaceDebrisRequest request, CancellationToken ct = default)
    {
        var entity = await _uow.SpaceDebris.GetByIdAsync(id, ct);
        if (entity is null) return null;
        entity.UpdateDetails(request.Name, request.Description);
        await _uow.SpaceDebris.UpdateAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return MapToDetail(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _uow.SpaceDebris.GetByIdAsync(id, ct);
        if (entity is null) return false;
        await _uow.SpaceDebris.DeleteAsync(id, ct);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> UpdatePositionAsync(Guid id, UpdatePositionRequest request, CancellationToken ct = default)
    {
        var entity = await _uow.SpaceDebris.GetByIdAsync(id, ct);
        if (entity is null) return false;
        var newPos = new OrbitalPosition(
            request.Altitude, request.Inclination, request.RightAscension,
            request.Eccentricity, request.Velocity, DateTime.UtcNow);
        entity.UpdatePosition(newPos);
        await _uow.SpaceDebris.UpdateAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IEnumerable<SpaceDebrisSummaryDto>> GetHighRiskAsync(CancellationToken ct = default)
    {
        var items = await _uow.SpaceDebris.GetHighRiskDebrisAsync(ct);
        return items.Select(MapToSummary);
    }

    public async Task<IEnumerable<SpaceDebrisSummaryDto>> GetByOrbitAsync(OrbitType orbit, CancellationToken ct = default)
    {
        var items = await _uow.SpaceDebris.GetByOrbitTypeAsync(orbit, ct);
        return items.Select(MapToSummary);
    }

    public async Task<IEnumerable<SpaceDebrisSummaryDto>> SearchAsync(string term, CancellationToken ct = default)
    {
        var items = await _uow.SpaceDebris.SearchAsync(term, ct);
        return items.Select(MapToSummary);
    }

    // ── Mappers ──────────────────────────────────────────────────

    private static SpaceDebrisSummaryDto MapToSummary(SpaceDebris d) => new(
        d.Id, d.CatalogNumber, d.Name, d.Type, d.OrbitType,
        d.SizeMeters, d.CollisionProbability, d.IsHighRisk(),
        MapPosition(d.CurrentPosition));

    private static SpaceDebrisDetailDto MapToDetail(SpaceDebris d) => new(
        d.Id, d.CatalogNumber, d.Name, d.Description, d.Type, d.OrbitType,
        d.SizeMeters, d.MassKg, d.OriginCountry, d.OriginMission, d.LaunchDate,
        d.CollisionProbability, d.IsHighRisk(),
        MapPosition(d.CurrentPosition),
        d.PositionHistory.Select(MapPosition),
        d.CreatedAt, d.UpdatedAt);

    private static OrbitalPositionDto MapPosition(OrbitalPosition p) => new(
        p.Altitude, p.Inclination, p.RightAscension, p.Eccentricity, p.Velocity, p.MeasuredAt);

    private static OrbitalPosition MapToOrbitalPosition(OrbitalPositionDto dto) => new(
        dto.Altitude, dto.Inclination, dto.RightAscension,
        dto.Eccentricity, dto.Velocity,
        dto.MeasuredAt == default ? DateTime.UtcNow : dto.MeasuredAt);
}
