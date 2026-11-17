using Microsoft.AspNetCore.Mvc;
using PortHub.API.DTOs;
using PortHub.API.Interfaces;
using PortHub.API.Filters;

namespace PortHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiKey]
public class SlotsController : ControllerBase
{
    private readonly ISlotService _slotService;

    public SlotsController(ISlotService slotService)
    {
        _slotService = slotService;
    }

    [HttpGet("disponibilidad")]
    public async Task<IActionResult> ConsultarDisponibilidad([FromQuery] int pista, [FromQuery] DateTime horario)
    {
        var disponible = await _slotService.EstaDisponibleAsync(pista, horario);
        return Ok(new { disponible, pista, horario });
    }

    [HttpPost("solicitudes")]
    public async Task<ActionResult<SlotResponseDto>> SolicitarSlot(SolicitudSlotDto dto)
    {
        if (!await _slotService.EstaDisponibleAsync(dto.Pista, dto.HorarioProgramado))
        {
            return Conflict(new ErrorResponseDto
            {
                Code = "SLOT_OCCUPIED",
                Message = "La pista ya se encuentra ocupada para el horario solicitado.",
                Details = $"Pista {dto.Pista} ocupada a las {dto.HorarioProgramado:yyyy-MM-dd HH:mm:ss} UTC"
            });
        }

        var slot = await _slotService.ReservarAsync(dto);
        return CreatedAtAction(nameof(GetByCodigoVuelo), new { codigoVuelo = slot.CodigoVuelo }, slot);
    }

    [HttpPost("cancelaciones")]
    public async Task<IActionResult> CancelarSlot(CancelacionSlotDto dto)
    {
        var slot = await _slotService.LiberarAsync(dto.CodigoVuelo);
        if (slot == null)
        {
            return NotFound(new ErrorResponseDto
            {
                Code = "SLOT_NOT_FOUND",
                Message = "No se encontró un slot activo asignado a ese código de vuelo.",
                Details = $"Vuelo: {dto.CodigoVuelo}"
            });
        }

        return Ok(new
        {
            mensaje = "Slot liberado correctamente",
            codigoVuelo = slot.CodigoVuelo,
            estado = slot.Estado
        });
    }

    [HttpGet]
    public async Task<ActionResult<SlotResponseDto>> GetByCodigoVuelo([FromQuery] string codigoVuelo)
    {
        var slot = await _slotService.ObtenerPorCodigoVueloAsync(codigoVuelo);
        if (slot == null)
            return NotFound(new ErrorResponseDto
            {
                Code = "SLOT_NOT_FOUND",
                Message = "No existe un slot para ese código de vuelo.",
                Details = $"Vuelo: {codigoVuelo}"
            });
        return Ok(slot);
    }
}