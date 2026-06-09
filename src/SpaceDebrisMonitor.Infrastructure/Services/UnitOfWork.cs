using Microsoft.EntityFrameworkCore.Storage;
using SpaceDebrisMonitor.Domain.Interfaces;
using SpaceDebrisMonitor.Infrastructure.Data;
using SpaceDebrisMonitor.Infrastructure.Repositories;

namespace SpaceDebrisMonitor.Infrastructure.Services;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;
    private IDbContextTransaction? _transaction;

    public ISpaceDebrisRepository SpaceDebris { get; }
    public ISatelliteRepository Satellites { get; }
    public IAlertRepository Alerts { get; }
    public IUserRepository Users { get; }

    public UnitOfWork(AppDbContext db)
    {
        _db = db;
        SpaceDebris = new SpaceDebrisRepository(db);
        Satellites = new SatelliteRepository(db);
        Alerts = new AlertRepository(db);
        Users = new UserRepository(db);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _db.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default) =>
        _transaction = await _db.Database.BeginTransactionAsync(ct);

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is not null) await _transaction.CommitAsync(ct);
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is not null) await _transaction.RollbackAsync(ct);
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _db.Dispose();
    }
}
