using Microsoft.EntityFrameworkCore;
using SpaceDebrisMonitor.Domain.Entities;
using SpaceDebrisMonitor.Domain.Enums;
using SpaceDebrisMonitor.Domain.Interfaces;
using SpaceDebrisMonitor.Infrastructure.Data;

namespace SpaceDebrisMonitor.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _db;
    protected readonly DbSet<T> _set;

    public Repository(AppDbContext db) { _db = db; _set = db.Set<T>(); }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _set.FindAsync(new object[] { id }, ct);

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default) =>
        await _set.ToListAsync(ct);

    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await _set.AddAsync(entity, ct);
        return entity;
    }

    public virtual Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        _set.Update(entity);
        return Task.CompletedTask;
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await GetByIdAsync(id, ct);
        if (entity is not null) entity.SoftDelete();
    }

    public virtual async Task<int> CountAsync(CancellationToken ct = default) =>
        await _set.CountAsync(ct);
}

// ── SpaceDebris ────────────────────────────────────────────────────────────

public class SpaceDebrisRepository : Repository<SpaceDebris>, ISpaceDebrisRepository
{
    public SpaceDebrisRepository(AppDbContext db) : base(db) { }

    public async Task<SpaceDebris?> GetByCatalogNumberAsync(string catalogNumber, CancellationToken ct = default) =>
        await _set.FirstOrDefaultAsync(x => x.CatalogNumber == catalogNumber, ct);

    public async Task<IEnumerable<SpaceDebris>> GetByOrbitTypeAsync(OrbitType orbitType, CancellationToken ct = default) =>
        await _set.Where(x => x.OrbitType == orbitType).ToListAsync(ct);

    public async Task<IEnumerable<SpaceDebris>> GetHighRiskDebrisAsync(CancellationToken ct = default) =>
        await _set.Where(x => x.CollisionProbability >= 0.01).OrderByDescending(x => x.CollisionProbability).ToListAsync(ct);

    public async Task<IEnumerable<SpaceDebris>> GetByTypeAsync(DebrisType type, CancellationToken ct = default) =>
        await _set.Where(x => x.Type == type).ToListAsync(ct);

    public async Task<IEnumerable<SpaceDebris>> SearchAsync(string term, CancellationToken ct = default) =>
        await _set.Where(x => x.Name.Contains(term) || x.CatalogNumber.Contains(term) ||
            (x.OriginCountry != null && x.OriginCountry.Contains(term))).ToListAsync(ct);
}

// ── Satellite ──────────────────────────────────────────────────────────────

public class SatelliteRepository : Repository<Satellite>, ISatelliteRepository
{
    public SatelliteRepository(AppDbContext db) : base(db) { }

    public async Task<Satellite?> GetByNoradIdAsync(string noradId, CancellationToken ct = default) =>
        await _set.FirstOrDefaultAsync(x => x.NoradId == noradId, ct);

    public async Task<IEnumerable<Satellite>> GetOperationalSatellitesAsync(CancellationToken ct = default) =>
        await _set.Where(x => x.Status == SatelliteStatus.Operational).Include(x => x.Sensors).ToListAsync(ct);

    public async Task<Satellite?> GetWithSensorsAsync(Guid id, CancellationToken ct = default) =>
        await _set.Include(x => x.Sensors).FirstOrDefaultAsync(x => x.Id == id, ct);
}

// ── Alert ──────────────────────────────────────────────────────────────────

public class AlertRepository : Repository<Alert>, IAlertRepository
{
    public AlertRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<Alert>> GetActiveAlertsAsync(CancellationToken ct = default) =>
        await _set.Include(a => a.SpaceDebris).Include(a => a.Satellite)
            .Where(x => x.Status == AlertStatus.Active || x.Status == AlertStatus.Acknowledged)
            .OrderByDescending(x => x.Severity).ToListAsync(ct);

    public async Task<IEnumerable<Alert>> GetByDebrisIdAsync(Guid debrisId, CancellationToken ct = default) =>
        await _set.Include(a => a.SpaceDebris).Include(a => a.Satellite)
            .Where(x => x.SpaceDebrisId == debrisId).ToListAsync(ct);

    public async Task<IEnumerable<Alert>> GetBySeverityAsync(AlertSeverity severity, CancellationToken ct = default) =>
        await _set.Include(a => a.SpaceDebris).Where(x => x.Severity == severity).ToListAsync(ct);

    public async Task<IEnumerable<Alert>> GetAlertsInPeriodAsync(DateTime from, DateTime to, CancellationToken ct = default) =>
        await _set.Include(a => a.SpaceDebris).Include(a => a.Satellite)
            .Where(x => x.CreatedAt >= from && x.CreatedAt <= to).ToListAsync(ct);
}

// ── User ───────────────────────────────────────────────────────────────────

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext db) : base(db) { }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        await _set.FirstOrDefaultAsync(x => x.Username == username, ct);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await _set.FirstOrDefaultAsync(x => x.Email == email.ToLowerInvariant(), ct);

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default) =>
        await _set.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken, ct);
}
