using Microsoft.AspNetCore.Mvc;
using PortHub.API.DTOs;
using PortHub.API.Interfaces;
using PortHub.API.Filters;
using System.ComponentModel.DataAnnotations;
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
    public async Task<IActionResult> ConsultarDisponibilidad([FromQuery, Required, Range(1, 10)] int? pista, [FromQuery, Required] DateTime? horario)
    {
        var disponible = await _slotService.EstaDisponibleAsync(pista!.Value, horario!.Value);
        return Ok(new { disponible, pista = pista.Value, horario = horario.Value });
    }

    [HttpPost("solicitudes")]
    public async Task<ActionResult<SlotResponseDto>> SolicitarSlot(SolicitudSlotDto dto)
    {
        var resultado = await _slotService.ReservarAsync(dto);

        switch (resultado.Estado)
        {
            case EstadoReservaSlot.HorarioPasado:
                return UnprocessableEntity(new ErrorResponseDto
                {
                    Code = "HORARIO_PASADO",
                    Message = "El horario solicitado ya pasó. Solo se pueden reservar slots a futuro.",
                    Details = resultado.Detalle
                });
            case EstadoReservaSlot.AerolineaNoExiste:
                return UnprocessableEntity(new ErrorResponseDto
                {
                    Code = "AEROLINEA_NO_EXISTE",
                    Message = "La aerolínea no está registrada en el aeropuerto.",
                    Details = resultado.Detalle
                });
            case EstadoReservaSlot.VueloConSlotActivo:
                return Conflict(new ErrorResponseDto
                {
                    Code = "VUELO_CON_SLOT_ACTIVO",
                    Message = "El vuelo ya tiene un slot activo. Cancele ese slot antes de pedir otro.",
                    Details = resultado.Detalle
                });
            case EstadoReservaSlot.PistaOcupada:
            case EstadoReservaSlot.GateOcupado:
                return Conflict(new ErrorResponseDto
                {
                    Code = "SLOT_OCCUPIED",
                    Message = "La pista o gate no se encuentra disponible para el horario solicitado.",
                    Details = resultado.Detalle
                });
        }
        return CreatedAtAction(nameof(GetByCodigoVuelo), new { codigoVuelo = resultado.Slot!.CodigoVuelo }, resultado.Slot);
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