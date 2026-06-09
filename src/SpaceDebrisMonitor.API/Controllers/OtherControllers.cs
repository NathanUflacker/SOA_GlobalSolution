using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpaceDebrisMonitor.Application.DTOs;
using SpaceDebrisMonitor.Application.Interfaces;
using SpaceDebrisMonitor.Domain.Enums;

namespace SpaceDebrisMonitor.API.Controllers;

// ── Alerts ─────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[Tags("Alertas")]
public class AlertsController : ControllerBase
{
    private readonly IAlertService _service;
    public AlertsController(IAlertService service) => _service = service;

    /// <summary>Retorna todos os alertas ativos/reconhecidos ordenados por severidade.</summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActive(CancellationToken ct = default) =>
        Ok(await _service.GetActiveAlertsAsync(ct));

    /// <summary>Retorna alertas associados a um detrito específico.</summary>
    [HttpGet("debris/{debrisId:guid}")]
    public async Task<IActionResult> GetByDebris(Guid debrisId, CancellationToken ct = default) =>
        Ok(await _service.GetByDebrisIdAsync(debrisId, ct));

    /// <summary>Retorna alertas dentro de um intervalo de data e hora.</summary>
    [HttpGet("period")]
    public async Task<IActionResult> GetByPeriod([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct = default) =>
        Ok(await _service.GetAlertsInPeriodAsync(from, to, ct));

    /// <summary>Cria um novo alerta de risco de colisão.</summary>
    [HttpPost]
    [Authorize(Policy = "OperatorOrAbove")]
    public async Task<IActionResult> Create([FromBody] CreateAlertRequest request, CancellationToken ct = default)
    {
        var result = await _service.CreateAlertAsync(request, ct);
        return CreatedAtAction(nameof(GetActive), result);
    }

    /// <summary>Reconhece um alerta.</summary>
    [HttpPatch("{id:guid}/acknowledge")]
    [Authorize(Policy = "AnalystOrAbove")]
    public async Task<IActionResult> Acknowledge(Guid id, CancellationToken ct = default)
    {
        var ok = await _service.AcknowledgeAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>Resolve um alerta com observações.</summary>
    [HttpPatch("{id:guid}/resolve")]
    [Authorize(Policy = "OperatorOrAbove")]
    public async Task<IActionResult> Resolve(Guid id, [FromBody] ResolveAlertRequest request, CancellationToken ct = default)
    {
        var ok = await _service.ResolveAsync(id, request, ct);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>Marca um alerta como falso positivo.</summary>
    [HttpPatch("{id:guid}/false-positive")]
    [Authorize(Policy = "OperatorOrAbove")]
    public async Task<IActionResult> FalsePositive(Guid id, [FromBody] string reason, CancellationToken ct = default)
    {
        var ok = await _service.MarkFalsePositiveAsync(id, reason, ct);
        return ok ? NoContent() : NotFound();
    }
}

// ── Satellites ─────────────────────────────────────────────────────────────

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[Tags("Satélites")]
public class SatellitesController : ControllerBase
{
    private readonly ISatelliteService _service;
    private readonly ITrajectoryPredictionService _trajectory;

    public SatellitesController(ISatelliteService service, ITrajectoryPredictionService trajectory)
    {
        _service = service;
        _trajectory = trajectory;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken ct = default) =>
        Ok(await _service.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create([FromBody] CreateSatelliteRequest request, CancellationToken ct = default)
    {
        var result = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = "OperatorOrAbove")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] SatelliteStatus status, CancellationToken ct = default)
    {
        var ok = await _service.UpdateStatusAsync(id, status, ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var ok = await _service.DeleteAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>Estima a probabilidade de colisão entre um detrito e este satélite.</summary>
    [HttpGet("{id:guid}/collision-risk/{debrisId:guid}")]
    public async Task<IActionResult> CollisionRisk(Guid id, Guid debrisId, CancellationToken ct = default)
    {
        var prob = await _trajectory.EstimateCollisionProbabilityAsync(debrisId, id, ct);
        return Ok(new { satelliteId = id, debrisId, collisionProbability = prob, riskLevel = prob >= 0.05 ? "CRÍTICO" : prob >= 0.01 ? "ALTO" : prob >= 0.001 ? "MÉDIO" : "BAIXO" });
    }
}

// ── Auth ───────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Autenticação")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;
    public AuthController(IAuthService service) => _service = service;

    /// <summary>Registra uma nova conta de usuário (perfil Viewer por padrão).</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct = default)
    {
        var result = await _service.RegisterAsync(request, ct);
        return Ok(result);
    }

    /// <summary>Autentica e retorna tokens JWT de acesso e renovação.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct = default)
    {
        var result = await _service.LoginAsync(request, ct);
        return Ok(result);
    }

    /// <summary>Emite um novo token de acesso usando um token de renovação válido.</summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct = default)
    {
        var result = await _service.RefreshTokenAsync(request, ct);
        return Ok(result);
    }

    /// <summary>Revoga o token de renovação (logout).</summary>
    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke([FromBody] string refreshToken, CancellationToken ct = default)
    {
        await _service.RevokeTokenAsync(refreshToken, ct);
        return NoContent();
    }
}

// ── Dashboard ──────────────────────────────────────────────────────────────

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Painel")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;
    public DashboardController(IDashboardService service) => _service = service;

    /// <summary>Retorna estatísticas gerais do sistema para o painel de monitoramento.</summary>
    [HttpGet("stats")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStats(CancellationToken ct = default) =>
        Ok(await _service.GetStatsAsync(ct));
}
