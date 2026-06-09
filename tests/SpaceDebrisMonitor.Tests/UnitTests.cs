using FluentAssertions;
using Moq;
using SpaceDebrisMonitor.Application.DTOs;
using SpaceDebrisMonitor.Application.Interfaces;
using SpaceDebrisMonitor.Application.Services;
using SpaceDebrisMonitor.Domain.Entities;
using SpaceDebrisMonitor.Domain.Enums;
using SpaceDebrisMonitor.Domain.Interfaces;
using SpaceDebrisMonitor.Domain.ValueObjects;
using Xunit;

namespace SpaceDebrisMonitor.Tests.Unit;

// ── OrbitalPosition Value Object Tests ────────────────────────────────────

public class OrbitalPositionTests
{
    private static OrbitalPosition CreatePosition(double altitude = 550, double velocity = 7.6) =>
        new(altitude, 53.0, 120.0, 0.001, velocity, DateTime.UtcNow);

    [Fact]
    public void Constructor_ValidParams_ShouldCreatePosition()
    {
        var pos = CreatePosition();
        pos.Altitude.Should().Be(550);
        pos.Velocity.Should().Be(7.6);
    }

    [Fact]
    public void Constructor_NegativeAltitude_ShouldThrow()
    {
        var act = () => new OrbitalPosition(-1, 53, 120, 0.001, 7.6, DateTime.UtcNow);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DistanceTo_SamePosition_ShouldReturnNearZero()
    {
        var pos = CreatePosition();
        var distance = pos.DistanceTo(pos);
        distance.Should().BeApproximately(0, 0.01);
    }

    [Fact]
    public void DistanceTo_DifferentAltitudes_ShouldReturnPositiveDistance()
    {
        var pos1 = CreatePosition(500);
        var pos2 = CreatePosition(800);
        pos1.DistanceTo(pos2).Should().BePositive();
    }

    [Fact]
    public void Equality_SameValues_ShouldBeEqual()
    {
        var t = DateTime.UtcNow;
        var p1 = new OrbitalPosition(550, 53, 120, 0.001, 7.6, t);
        var p2 = new OrbitalPosition(550, 53, 120, 0.001, 7.6, t);
        p1.Should().Be(p2);
    }
}

// ── SpaceDebris Entity Tests ───────────────────────────────────────────────

public class SpaceDebrisTests
{
    private static OrbitalPosition MakePos() =>
        new(550, 53, 120, 0.001, 7.6, DateTime.UtcNow);

    private static SpaceDebris MakeDebris() =>
        new("2024-001A", "Starlink Debris", DebrisType.Fragment, OrbitType.LEO,
            0.1, 0.5, MakePos(), "USA", "Starlink-30", new DateTime(2020, 1, 1));

    [Fact]
    public void Constructor_ValidArgs_ShouldCreate()
    {
        var d = MakeDebris();
        d.CatalogNumber.Should().Be("2024-001A");
        d.CollisionProbability.Should().Be(0);
        d.PositionHistory.Should().HaveCount(1);
    }

    [Fact]
    public void Constructor_EmptyCatalog_ShouldThrow()
    {
        var act = () => new SpaceDebris("", "Name", DebrisType.Fragment, OrbitType.LEO,
            0.1, 0.5, MakePos());
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdatePosition_AddsToHistory()
    {
        var debris = MakeDebris();
        var newPos = new OrbitalPosition(545, 53, 125, 0.001, 7.61, DateTime.UtcNow);
        debris.UpdatePosition(newPos);
        debris.PositionHistory.Should().HaveCount(2);
        debris.CurrentPosition.Should().Be(newPos);
    }

    [Fact]
    public void IsHighRisk_BelowThreshold_ReturnsFalse()
    {
        var debris = MakeDebris();
        debris.IsHighRisk().Should().BeFalse();
    }
}

// ── Alert Entity Tests ─────────────────────────────────────────────────────

public class AlertTests
{
    private static Alert MakeAlert(double distance = 3.0, double prob = 0.02) =>
        new(Guid.NewGuid(), Alert.DetermineSeverity(distance, prob),
            "Close Approach Warning", "Debris approaching satellite at 3km.",
            distance, prob, null, DateTime.UtcNow.AddHours(2));

    [Fact]
    public void DetermineSeverity_CloseHighProb_ReturnsCritical()
    {
        var sev = Alert.DetermineSeverity(0.5, 0.1);
        sev.Should().Be(AlertSeverity.Critical);
    }

    [Fact]
    public void DetermineSeverity_FarLowProb_ReturnsLow()
    {
        var sev = Alert.DetermineSeverity(100, 0.0001);
        sev.Should().Be(AlertSeverity.Low);
    }

    [Fact]
    public void Acknowledge_ActiveAlert_ChangesStatus()
    {
        var alert = MakeAlert();
        alert.Acknowledge();
        alert.Status.Should().Be(AlertStatus.Acknowledged);
        alert.AcknowledgedAt.Should().NotBeNull();
    }

    [Fact]
    public void Acknowledge_AlreadyResolved_ShouldThrow()
    {
        var alert = MakeAlert();
        alert.Resolve("resolved");
        var act = () => alert.Acknowledge();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Resolve_WithNotes_ChangesStatusAndTime()
    {
        var alert = MakeAlert();
        alert.Resolve("Maneuver executed successfully.");
        alert.Status.Should().Be(AlertStatus.Resolved);
        alert.ResolutionNotes.Should().Be("Maneuver executed successfully.");
    }

    [Fact]
    public void TimeUntilApproach_FutureApproach_ReturnsPositiveSpan()
    {
        var alert = MakeAlert();
        alert.TimeUntilApproach().Should().NotBeNull();
        alert.TimeUntilApproach()!.Value.TotalMinutes.Should().BePositive();
    }
}

// ── Satellite Entity Tests ─────────────────────────────────────────────────

public class SatelliteTests
{
    private static OrbitalPosition MakePos() =>
        new(560, 53, 125, 0.0001, 7.59, DateTime.UtcNow);

    [Fact]
    public void Constructor_ValidArgs_ShouldCreate()
    {
        var sat = new Satellite("SAT-1", "25544", "FIAP Space Agency", MakePos(), DateTime.Today);
        sat.IsOperational().Should().BeTrue();
        sat.Status.Should().Be(SatelliteStatus.Operational);
    }

    [Fact]
    public void UpdateStatus_Decommission_SetsDate()
    {
        var sat = new Satellite("SAT-1", "25544", "FIAP", MakePos(), DateTime.Today);
        sat.UpdateStatus(SatelliteStatus.Decommissioned);
        sat.Status.Should().Be(SatelliteStatus.Decommissioned);
        sat.DecommissionDate.Should().NotBeNull();
    }

    [Fact]
    public void GetRiskScore_LEO_ReturnsHigherRisk()
    {
        var sat = new Satellite("SAT-1", "25544", "FIAP", MakePos(), DateTime.Today);
        sat.GetRiskScore().Should().Be(0.35);
    }
}

// ── SpaceDebrisService Tests (with Moq) ────────────────────────────────────

public class SpaceDebrisServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<ISpaceDebrisRepository> _repoMock = new();
    private readonly SpaceDebrisService _service;

    public SpaceDebrisServiceTests()
    {
        _uowMock.Setup(u => u.SpaceDebris).Returns(_repoMock.Object);
        _service = new SpaceDebrisService(_uowMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResult()
    {
        var debris = new List<SpaceDebris>
        {
            new("2024-001A", "Test Debris", DebrisType.Fragment, OrbitType.LEO,
                0.1, 0.5, new OrbitalPosition(550, 53, 120, 0.001, 7.6, DateTime.UtcNow))
        };
        _repoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(debris);

        var result = await _service.GetAllAsync(1, 20);

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((SpaceDebris?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_DuplicateCatalog_ThrowsInvalidOperation()
    {
        var existing = new SpaceDebris("2024-001A", "Existing", DebrisType.Fragment, OrbitType.LEO,
            0.1, 0.5, new OrbitalPosition(550, 53, 120, 0.001, 7.6, DateTime.UtcNow));
        _repoMock.Setup(r => r.GetByCatalogNumberAsync("2024-001A", default)).ReturnsAsync(existing);

        var request = new CreateSpaceDebrisRequest("2024-001A", "New", null, DebrisType.Fragment,
            OrbitType.LEO, 0.1, 0.5,
            new OrbitalPositionDto(550, 53, 120, 0.001, 7.6, DateTime.UtcNow),
            null, null, null);

        var act = async () => await _service.CreateAsync(request);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
