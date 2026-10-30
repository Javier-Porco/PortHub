using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortHub.API.Data;
using PortHub.API.Domain.DTOs;
using PortHub.API.Domain.Entities;
using PortHub.API.Services;

namespace PortHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmbarquesController : ControllerBase
{
    private readonly PortHubDbContext _context;
    private readonly AeroLinkClient _aeroLinkClient;

    public EmbarquesController(PortHubDbContext context, AeroLinkClient aeroLinkClient)
    {
        _context = context;
        _aeroLinkClient = aeroLinkClient;
    }

    [HttpPost]
    public async Task<ActionResult<EmbarqueResponseDto>> RegistrarAcceso(RegistroEmbarqueDto dto)
    {
        // 1. Validar ticket llamando a la API de AeroLink
        var esValido = await _aeroLinkClient.ValidarTicketAsync(dto.NumeroTicket);

        if (!esValido)
        {
            return BadRequest(new ErrorResponseDto
            {
                Code = "INVALID_TICKET",
                Message = "El ticket de embarque no es válido o no fue confirmado por la aerolínea.",
                Details = $"Ticket rechazado: {dto.NumeroTicket}"
            });
        }

        // 2. Registrar el acceso en la DB
        var embarque = new Embarque
        {
            NumeroTicket = dto.NumeroTicket,
            Gate = dto.Gate,
            HorarioAcceso = DateTime.UtcNow
        };

        _context.Embarques.Add(embarque);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTrazabilidad), new { gate = embarque.Gate }, new EmbarqueResponseDto
        {
            Id = embarque.Id,
            NumeroTicket = embarque.NumeroTicket,
            Gate = embarque.Gate,
            HorarioAcceso = embarque.HorarioAcceso
        });
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmbarqueResponseDto>>> GetTrazabilidad([FromQuery] string? gate)
    {
        var query = _context.Embarques.AsQueryable();

        if (!string.IsNullOrEmpty(gate))
        {
            query = query.Where(e => e.Gate == gate);
        }

        var listado = await query
            .OrderByDescending(e => e.HorarioAcceso)
            .Select(e => new EmbarqueResponseDto
            {
                Id = e.Id,
                NumeroTicket = e.NumeroTicket,
                Gate = e.Gate,
                HorarioAcceso = e.HorarioAcceso
            })
            .ToListAsync();

        return Ok(listado);
    }
}