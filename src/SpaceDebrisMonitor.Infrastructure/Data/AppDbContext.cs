using Microsoft.EntityFrameworkCore;
using SpaceDebrisMonitor.Domain.Entities;

namespace SpaceDebrisMonitor.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<SpaceDebris> SpaceDebris => Set<SpaceDebris>();
    public DbSet<Satellite> Satellites => Set<Satellite>();
    public DbSet<Sensor> Sensors => Set<Sensor>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<SpaceDebris>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.CatalogNumber).IsRequired().HasMaxLength(20);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.HasIndex(x => x.CatalogNumber).IsUnique();
            e.OwnsOne(x => x.CurrentPosition, pos =>
            {
                pos.Property(p => p.Altitude).HasColumnName("Altitude");
                pos.Property(p => p.Inclination).HasColumnName("Inclination");
                pos.Property(p => p.RightAscension).HasColumnName("RightAscension");
                pos.Property(p => p.Eccentricity).HasColumnName("Eccentricity");
                pos.Property(p => p.Velocity).HasColumnName("Velocity");
                pos.Property(p => p.MeasuredAt).HasColumnName("MeasuredAt");
            });
            // PositionHistory é uma lista de Value Objects — ignorada pelo EF Core.
            // O histórico em memória continua funcionando durante o ciclo de vida do objeto.
            e.Ignore(x => x.PositionHistory);
            e.HasMany(x => x.Alerts).WithOne(a => a.SpaceDebris).HasForeignKey(a => a.SpaceDebrisId);
        });

        mb.Entity<Satellite>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.NoradId).IsRequired().HasMaxLength(10);
            e.HasIndex(x => x.NoradId).IsUnique();
            e.OwnsOne(x => x.CurrentPosition, pos =>
            {
                pos.Property(p => p.Altitude).HasColumnName("Altitude");
                pos.Property(p => p.Inclination).HasColumnName("Inclination");
                pos.Property(p => p.RightAscension).HasColumnName("RightAscension");
                pos.Property(p => p.Eccentricity).HasColumnName("Eccentricity");
                pos.Property(p => p.Velocity).HasColumnName("Velocity");
                pos.Property(p => p.MeasuredAt).HasColumnName("MeasuredAt");
            });
            e.HasMany(x => x.Sensors).WithOne(s => s.Satellite).HasForeignKey(s => s.SatelliteId);
        });

        mb.Entity<Alert>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(300);
            e.HasOne(x => x.Satellite).WithMany(s => s.GeneratedAlerts)
             .HasForeignKey(x => x.SatelliteId).IsRequired(false);
        });

        mb.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Username).IsRequired().HasMaxLength(100);
            e.Property(x => x.Email).IsRequired().HasMaxLength(200);
            e.HasIndex(x => x.Username).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
        });

        mb.Entity<SpaceDebris>().HasQueryFilter(x => !x.IsDeleted);
        mb.Entity<Satellite>().HasQueryFilter(x => !x.IsDeleted);
        mb.Entity<Alert>().HasQueryFilter(x => !x.IsDeleted);
        mb.Entity<User>().HasQueryFilter(x => !x.IsDeleted);
    }
}
