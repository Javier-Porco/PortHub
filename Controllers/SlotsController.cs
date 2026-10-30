using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortHub.API.Data;
using PortHub.API.Domain.DTOs;
using PortHub.API.Domain.Entities;
using PortHub.API.Domain.Enums;

namespace PortHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SlotsController : ControllerBase
{
    private readonly PortHubDbContext _context;

    public SlotsController(PortHubDbContext context)
    {
        _context = context;
    }

    // 1. Consultar disponibilidad de slot
    [HttpGet("disponibilidad")]
    public async Task<IActionResult> ConsultarDisponibilidad([FromQuery] int pista, [FromQuery] DateTime horario)
    {
        var ocupado = await _context.Slots
            .AnyAsync(s => s.Pista == pista 
                        && s.HorarioProgramado == horario 
                        && s.Estado != EstadoSlot.Liberado);

        return Ok(new
        {
            disponible = !ocupado,
            pista = pista,
            horario = horario
        });
    }

    // 2. Solicitar reserva de slot
    [HttpPost("solicitudes")]
    public async Task<ActionResult<SlotResponseDto>> SolicitarSlot(SolicitudSlotDto dto)
    {
        // Validar que la pista no esté ocupada en ese horario
        var existeConflicto = await _context.Slots
            .AnyAsync(s => s.Pista == dto.Pista 
                        && s.HorarioProgramado == dto.HorarioProgramado 
                        && s.Estado != EstadoSlot.Liberado);

        if (existeConflicto)
        {
            return Conflict(new ErrorResponseDto
            {
                Code = "SLOT_OCCUPIED",
                Message = "La pista ya se encuentra ocupada para el horario solicitado.",
                Details = $"Pista {dto.Pista} ocupada a las {dto.HorarioProgramado:yyyy-MM-dd HH:mm:ss} UTC"
            });
        }

        var slot = new Slot
        {
            CodigoVuelo = dto.CodigoVuelo,
            CodigoAerolinea = dto.CodigoAerolinea,
            HorarioProgramado = dto.HorarioProgramado,
            Pista = dto.Pista,
            Gate = dto.Gate,
            Estado = EstadoSlot.Reservado
        };

        _context.Slots.Add(slot);
        await _context.SaveChangesAsync();

        var response = new SlotResponseDto
        {
            Id = slot.Id,
            CodigoVuelo = slot.CodigoVuelo,
            CodigoAerolinea = slot.CodigoAerolinea,
            HorarioProgramado = slot.HorarioProgramado,
            Pista = slot.Pista,
            Gate = slot.Gate,
            Estado = slot.Estado.ToString().ToLower()
        };

        return CreatedAtAction(nameof(GetByCodigoVuelo), new { codigoVuelo = slot.CodigoVuelo }, response);
    }

    // 3. Notificar cancelación (Liberar slot)
    [HttpPost("cancelaciones")]
    public async Task<IActionResult> CancelarSlot(CancelacionSlotDto dto)
    {
        var slot = await _context.Slots
            .FirstOrDefaultAsync(s => s.CodigoVuelo == dto.CodigoVuelo && s.Estado != EstadoSlot.Liberado);

        if (slot == null)
        {
            return NotFound(new ErrorResponseDto
            {
                Code = "SLOT_NOT_FOUND",
                Message = "No se encontró un slot activo asignado a ese código de vuelo.",
                Details = $"Vuelo: {dto.CodigoVuelo}"
            });
        }

        slot.Estado = EstadoSlot.Liberado;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Slot liberado correctamente",
            codigoVuelo = slot.CodigoVuelo,
            estado = slot.Estado.ToString().ToLower()
        });
    }

    // 4. Obtener slot por código de vuelo (Para el Desafío Adicional)
    [HttpGet]
    public async Task<ActionResult<SlotResponseDto>> GetByCodigoVuelo([FromQuery] string codigoVuelo)
    {
        var slot = await _context.Slots
            .Where(s => s.CodigoVuelo == codigoVuelo)
            .OrderByDescending(s => s.FechaCreacion)
            .FirstOrDefaultAsync();

        if (slot == null) return NotFound();

        return Ok(new SlotResponseDto
        {
            Id = slot.Id,
            CodigoVuelo = slot.CodigoVuelo,
            CodigoAerolinea = slot.CodigoAerolinea,
            HorarioProgramado = slot.HorarioProgramado,
            Pista = slot.Pista,
            Gate = slot.Gate,
            Estado = slot.Estado.ToString().ToLower()
        });
    }
}