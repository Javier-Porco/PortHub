using Microsoft.AspNetCore.Mvc;
using PortHub.API.DTOs;
using PortHub.API.Filters;
using PortHub.API.Interfaces;

namespace PortHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiKey]
public class EmbarquesController : ControllerBase
{
    private readonly IEmbarqueService _embarqueService;
    private readonly ILogger<EmbarquesController> _logger;

    public EmbarquesController(IEmbarqueService embarqueService, ILogger<EmbarquesController> logger)
    {
        _embarqueService = embarqueService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<EmbarqueResponseDto>> RegistrarAcceso(RegistroEmbarqueDto dto)
    {
        ResultadoRegistroEmbarque resultado;
        try
        {
            resultado = await _embarqueService.RegistrarAsync(dto);
        }
        catch (HttpRequestException ex)
        {
            // AeroLink está caído, tardó más de 5 segundos o respondió con error.
            // Igual que en la materia (IntegracionController): la API de la que dependemos falló.
            _logger.LogError(ex, "No se pudo validar el ticket {Ticket} con AeroLink", dto.NumeroTicket);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, Error(
                "AEROLINK_NO_DISPONIBLE",
                "No se pudo validar el ticket porque la API de AeroLink no está disponible.",
                "Intente nuevamente en unos minutos. El pasajero no fue registrado."));
        }

        switch (resultado.Estado)
        {
            case EstadoRegistroEmbarque.TicketYaUtilizado:
                return Conflict(Error("TICKET_YA_UTILIZADO", "El ticket ya fue usado para ingresar.", resultado.Detalle));
            case EstadoRegistroEmbarque.TicketInvalido:
                return UnprocessableEntity(Error("TICKET_INVALIDO", "El ticket no es válido.", resultado.Detalle));
            case EstadoRegistroEmbarque.VueloSinSlot:
                return UnprocessableEntity(Error("VUELO_SIN_SLOT", "El vuelo del ticket no tiene un slot activo.", resultado.Detalle));
            case EstadoRegistroEmbarque.GateIncorrecto:
                return UnprocessableEntity(Error("GATE_INCORRECTO", "El pasajero no está en el gate de su vuelo.", resultado.Detalle));
        }

        return CreatedAtAction(nameof(GetTrazabilidad), new { gate = resultado.Embarque!.Gate }, resultado.Embarque);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmbarqueResponseDto>>> GetTrazabilidad([FromQuery] string? gate)
    {
        return Ok(await _embarqueService.ObtenerTrazabilidadAsync(gate));
    }

    private static ErrorResponseDto Error(string code, string message, string details) =>
        new() { Code = code, Message = message, Details = details };
}