using Microsoft.AspNetCore.Mvc;
using PortHub.API.DTOs;
using PortHub.API.Interfaces;
using PortHub.API.Filters;

namespace PortHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiKey]
public class EmbarquesController : ControllerBase
{
    private readonly IEmbarqueService _embarqueService;

    public EmbarquesController(IEmbarqueService embarqueService)
    {
        _embarqueService = embarqueService;
    }

    [HttpPost]
    public async Task<ActionResult<EmbarqueResponseDto>> RegistrarAcceso(RegistroEmbarqueDto dto)
    {
        var embarque = await _embarqueService.RegistrarAsync(dto);
        if (embarque == null)
        {
            return BadRequest(new ErrorResponseDto
            {
                Code = "INVALID_TICKET",
                Message = "El ticket de embarque no es válido o no fue confirmado por la aerolínea.",
                Details = $"Ticket rechazado: {dto.NumeroTicket}"
            });
        }

        return CreatedAtAction(nameof(GetTrazabilidad), new { gate = embarque.Gate }, embarque);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmbarqueResponseDto>>> GetTrazabilidad([FromQuery] string? gate)
    {
        return Ok(await _embarqueService.ObtenerTrazabilidadAsync(gate));
    }
}