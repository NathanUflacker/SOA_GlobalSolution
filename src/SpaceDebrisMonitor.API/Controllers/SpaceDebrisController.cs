using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpaceDebrisMonitor.Application.DTOs;
using SpaceDebrisMonitor.Application.Interfaces;
using SpaceDebrisMonitor.Domain.Enums;

namespace SpaceDebrisMonitor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[Tags("Detritos Espaciais")]
public class SpaceDebrisController : ControllerBase
{
    private readonly ISpaceDebrisService _service;
    private readonly ITrajectoryPredictionService _trajectory;

    public SpaceDebrisController(ISpaceDebrisService service, ITrajectoryPredictionService trajectory)
    {
        _service = service;
        _trajectory = trajectory;
    }

    /// <summary>Retorna uma lista paginada de todos os detritos rastreados.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>Retorna os detalhes completos de um detrito específico.</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result is null ? NotFound(new { message = $"Detrito {id} não encontrado." }) : Ok(result);
    }

    /// <summary>Registra um novo detrito no sistema de rastreamento.</summary>
    [HttpPost]
    [Authorize(Policy = "OperatorOrAbove")]
    public async Task<IActionResult> Create([FromBody] CreateSpaceDebrisRequest request, CancellationToken ct = default)
    {
        var result = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Atualiza o nome e a descrição de um registro de detrito.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "OperatorOrAbove")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSpaceDebrisRequest request, CancellationToken ct = default)
    {
        var result = await _service.UpdateAsync(id, request, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Remove logicamente um registro de detrito.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var deleted = await _service.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>Atualiza a posição orbital atual de um detrito.</summary>
    [HttpPatch("{id:guid}/position")]
    [Authorize(Policy = "OperatorOrAbove")]
    public async Task<IActionResult> UpdatePosition(Guid id, [FromBody] UpdatePositionRequest request, CancellationToken ct = default)
    {
        var ok = await _service.UpdatePositionAsync(id, request, ct);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>Retorna todos os detritos classificados como alto risco (probabilidade de colisão >= 1%).</summary>
    [HttpGet("high-risk")]
    public async Task<IActionResult> GetHighRisk(CancellationToken ct = default) =>
        Ok(await _service.GetHighRiskAsync(ct));

    /// <summary>Filtra detritos por tipo de órbita (LEO, MEO, GEO etc.).</summary>
    [HttpGet("by-orbit/{orbit}")]
    public async Task<IActionResult> GetByOrbit(OrbitType orbit, CancellationToken ct = default) =>
        Ok(await _service.GetByOrbitAsync(orbit, ct));

    /// <summary>Busca textual por número de catálogo, nome e país de origem.</summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string term, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(term)) return BadRequest(new { message = "Termo de busca obrigatório." });
        return Ok(await _service.SearchAsync(term, ct));
    }

    /// <summary>Predição de trajetória por IA para as próximas N horas.</summary>
    [HttpGet("{id:guid}/trajectory")]
    public async Task<IActionResult> PredictTrajectory(Guid id, [FromQuery] int hoursAhead = 24, CancellationToken ct = default)
    {
        var predictions = await _trajectory.PredictTrajectoryAsync(id, hoursAhead, ct);
        return Ok(new { debrisId = id, hoursAhead, predictions });
    }
}
