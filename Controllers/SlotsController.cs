using Microsoft.AspNetCore.Mvc;
using PortHub.API.DTOs;
using PortHub.API.Interfaces;
using PortHub.API.Filters;
using System.Net;

namespace PortHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiKey]
public class SlotsController : ControllerBase
{
    private readonly ISlotService _slotService;
    private readonly ILogger<SlotsController> _logger;

    public SlotsController(ISlotService slotService, ILogger<SlotsController> logger)
    {
        _slotService = slotService;
        _logger = logger;
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

    // PATCH: api/slots/{slotId}/confirmacion
    // El aeropuerto confirma un slot reservado y se lo avisa a AeroLink (callback).
    [HttpPatch("{slotId:guid}/confirmacion")]
    public async Task<ActionResult<SlotResponseDto>> ConfirmarSlot(Guid slotId)
    {
        ResultadoConfirmacionSlot resultado;
        try
        {
            resultado = await _slotService.ConfirmarAsync(slotId);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Conflict)
        {
            // AeroLink respondió que no acepta la confirmación (vuelo inexistente, cancelado, etc.).
            _logger.LogWarning(ex, "AeroLink rechazó la confirmación del slot {SlotId}", slotId);
            return UnprocessableEntity(new ErrorResponseDto
            {
                Code = "AEROLINK_RECHAZO_CONFIRMACION",
                Message = "AeroLink rechazó la confirmación. El slot sigue reservado.",
                Details = $"Slot: {slotId}. AeroLink respondió {(int?)ex.StatusCode}."
            });
        }
        catch (HttpRequestException ex)
        {
            // AeroLink está caído, tardó más de 5 segundos o respondió 5xx.
            _logger.LogError(ex, "No se pudo notificar a AeroLink la confirmación del slot {SlotId}", slotId);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new ErrorResponseDto
            {
                Code = "AEROLINK_NO_DISPONIBLE",
                Message = "No se pudo avisar a AeroLink. El slot sigue reservado; intente nuevamente.",
                Details = $"Slot: {slotId}"
            });
        }

        switch (resultado.Estado)
        {
            case EstadoConfirmacionSlot.NoEncontrado:
                return NotFound(new ErrorResponseDto
                {
                    Code = "SLOT_NOT_FOUND",
                    Message = "No existe un slot con ese id.",
                    Details = $"Slot: {slotId}"
                });
            case EstadoConfirmacionSlot.SlotLiberado:
                return Conflict(new ErrorResponseDto
                {
                    Code = "SLOT_LIBERADO",
                    Message = "El slot fue liberado y ya no se puede confirmar.",
                    Details = $"Slot: {slotId}"
                });
            case EstadoConfirmacionSlot.YaConfirmado:
                return Conflict(new ErrorResponseDto
                {
                    Code = "SLOT_YA_CONFIRMADO",
                    Message = "El slot ya estaba confirmado.",
                    Details = $"Slot: {slotId}"
                });
        }

        return Ok(resultado.Slot);
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