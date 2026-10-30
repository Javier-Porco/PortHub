using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortHub.API.Data;
using PortHub.API.Domain.DTOs;
using PortHub.API.Domain.Entities;

namespace PortHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AerolineasController : ControllerBase
{
    private readonly PortHubDbContext _context;

    public AerolineasController(PortHubDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AerolineaResponseDto>>> GetAll()
    {
        var aerolineas = await _context.Aerolineas
            .Select(a => new AerolineaResponseDto
            {
                Id = a.Id,
                Nombre = a.Nombre,
                Codigo = a.Codigo,
                Pais = a.Pais,
                DireccionBase = a.DireccionBase
            })
            .ToListAsync();

        return Ok(aerolineas);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AerolineaResponseDto>> GetById(int id)
    {
        var a = await _context.Aerolineas.FindAsync(id);
        if (a == null) return NotFound();

        return Ok(new AerolineaResponseDto
        {
            Id = a.Id,
            Nombre = a.Nombre,
            Codigo = a.Codigo,
            Pais = a.Pais,
            DireccionBase = a.DireccionBase
        });
    }

    [HttpPost]
    public async Task<ActionResult<AerolineaResponseDto>> Create(CrearAerolineaDto dto)
    {
        var existeCodigo = await _context.Aerolineas.AnyAsync(a => a.Codigo == dto.Codigo);
        if (existeCodigo)
        {
            return Conflict(new { message = "Ya existe una aerolínea con ese código." });
        }

        var aerolinea = new Aerolinea
        {
            Nombre = dto.Nombre,
            Codigo = dto.Codigo.ToUpper(),
            Pais = dto.Pais,
            DireccionBase = dto.DireccionBase
        };

        _context.Aerolineas.Add(aerolinea);
        await _context.SaveChangesAsync();

        var response = new AerolineaResponseDto
        {
            Id = aerolinea.Id,
            Nombre = aerolinea.Nombre,
            Codigo = aerolinea.Codigo,
            Pais = aerolinea.Pais,
            DireccionBase = aerolinea.DireccionBase
        };

        return CreatedAtAction(nameof(GetById), new { id = aerolinea.Id }, response);
    }

    [HttpPut("{id}")]
public async Task<IActionResult> Update(int id, ActualizarAerolineaDto dto)
{
    var aerolinea = await _context.Aerolineas.FindAsync(id);
    if (aerolinea == null) return NotFound();

    aerolinea.Nombre = dto.Nombre;
    aerolinea.Pais = dto.Pais;
    aerolinea.DireccionBase = dto.DireccionBase;

    await _context.SaveChangesAsync();
    return NoContent();
}
}