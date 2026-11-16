using Microsoft.AspNetCore.Mvc;
using PortHub.API.DTOs;
using PortHub.API.Interfaces;

namespace PortHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AerolineasController : ControllerBase
{
    private readonly IAerolineaService _aerolineaService;

    public AerolineasController(IAerolineaService aerolineaService)
    {
        _aerolineaService = aerolineaService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AerolineaResponseDto>>> GetAll()
    {
        return Ok(await _aerolineaService.ObtenerTodasAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AerolineaResponseDto>> GetById(int id)
    {
        var aerolinea = await _aerolineaService.ObtenerPorIdAsync(id);
        if (aerolinea == null) return NotFound();
        return Ok(aerolinea);
    }

    [HttpPost]
    public async Task<ActionResult<AerolineaResponseDto>> Create(CrearAerolineaDto dto)
    {
        if (await _aerolineaService.ExisteCodigoAsync(dto.Codigo))
            return Conflict(new { message = "Ya existe una aerolínea con ese código." });

        var creada = await _aerolineaService.CrearAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = creada.Id }, creada);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ActualizarAerolineaDto dto)
    {
        var actualizado = await _aerolineaService.ActualizarAsync(id, dto);
        if (!actualizado) return NotFound();
        return NoContent();
    }
}