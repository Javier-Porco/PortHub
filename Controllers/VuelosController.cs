using Microsoft.AspNetCore.Mvc;
using PortHub.API.Interfaces;

namespace PortHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VuelosController : ControllerBase
{
    private readonly ISlotService _slotService;

    public VuelosController(ISlotService slotService)
    {
        _slotService = slotService;
    }

    [HttpGet]
    public async Task<IActionResult> GetVuelosCombinados()
    {
        var slots = await _slotService.ObtenerActivosAsync();

        var resultado = slots.Select(s => new
        {
            codigoVuelo = s.CodigoVuelo,
            codigoAerolinea = s.CodigoAerolinea,
            horarioProgramado = s.HorarioProgramado,
            pistaAsignada = s.Pista,
            gateAsignado = s.Gate,
            estadoSlot = s.Estado
        });

        return Ok(resultado);
    }
}