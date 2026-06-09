using SpaceDebrisMonitor.Domain.Entities;
using SpaceDebrisMonitor.Domain.Enums;

namespace SpaceDebrisMonitor.Domain.Interfaces;

/// <summary>
/// Generic repository interface. Ensures all repos are testable via DI.
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
}

public interface ISpaceDebrisRepository : IRepository<SpaceDebris>
{
    Task<SpaceDebris?> GetByCatalogNumberAsync(string catalogNumber, CancellationToken ct = default);
    Task<IEnumerable<SpaceDebris>> GetByOrbitTypeAsync(OrbitType orbitType, CancellationToken ct = default);
    Task<IEnumerable<SpaceDebris>> GetHighRiskDebrisAsync(CancellationToken ct = default);
    Task<IEnumerable<SpaceDebris>> GetByTypeAsync(DebrisType type, CancellationToken ct = default);
    Task<IEnumerable<SpaceDebris>> SearchAsync(string term, CancellationToken ct = default);
}

public interface ISatelliteRepository : IRepository<Satellite>
{
    Task<Satellite?> GetByNoradIdAsync(string noradId, CancellationToken ct = default);
    Task<IEnumerable<Satellite>> GetOperationalSatellitesAsync(CancellationToken ct = default);
    Task<Satellite?> GetWithSensorsAsync(Guid id, CancellationToken ct = default);
}

public interface IAlertRepository : IRepository<Alert>
{
    Task<IEnumerable<Alert>> GetActiveAlertsAsync(CancellationToken ct = default);
    Task<IEnumerable<Alert>> GetByDebrisIdAsync(Guid debrisId, CancellationToken ct = default);
    Task<IEnumerable<Alert>> GetBySeverityAsync(AlertSeverity severity, CancellationToken ct = default);
    Task<IEnumerable<Alert>> GetAlertsInPeriodAsync(DateTime from, DateTime to, CancellationToken ct = default);
}

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
}

/// <summary>
/// Unit of Work — wraps a transaction across multiple repositories.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    ISpaceDebrisRepository SpaceDebris { get; }
    ISatelliteRepository Satellites { get; }
    IAlertRepository Alerts { get; }
    IUserRepository Users { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
