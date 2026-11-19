using Microsoft.AspNetCore.Mvc;
using PortHub.API.DTOs;
using PortHub.API.Filters;
using PortHub.API.Interfaces;

namespace PortHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiKey]
public class VuelosController : ControllerBase
{
    private readonly ISlotService _slotService;
    private readonly IAeroLinkService _aeroLinkService;

    public VuelosController(ISlotService slotService, IAeroLinkService aeroLinkService)
    {
        _slotService = slotService;
        _aeroLinkService = aeroLinkService;
    }

    [HttpGet]
    public async Task<IActionResult> GetVuelosActivos()
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

    // Desafío adicional: consulta unificada combinando PortHub (aeropuerto) + AeroLink (aerolínea)
    [HttpGet("combinados")]
    public async Task<IActionResult> GetVuelosCombinados()
    {
        var slots = await _slotService.ObtenerActivosAsync();

        List<VueloAeroLinkDto> vuelosAeroLink;
        try
        {
            vuelosAeroLink = await _aeroLinkService.ObtenerVuelosAsync();
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                code = "AEROLINK_UNAVAILABLE",
                message = "No se pudo consultar los vuelos en AeroLink.",
                details = ex.Message
            });
        }

        var vuelosPorCodigo = vuelosAeroLink
            .GroupBy(v => v.CodigoVuelo.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var combinados = slots.Select(s =>
        {
            vuelosPorCodigo.TryGetValue(s.CodigoVuelo.Trim(), out var vueloAero);

            return new VueloCombinadoDto
            {
                CodigoVuelo = s.CodigoVuelo,
                CodigoAerolinea = s.CodigoAerolinea,
                SlotId = s.Id.ToString(),
                PistaAsignada = s.Pista,
                GateAsignado = s.Gate,
                HorarioProgramado = s.HorarioProgramado,
                EstadoSlot = s.Estado.ToString(),
                Origen = vueloAero?.Origen,
                Destino = vueloAero?.Destino,
                HorarioSalidaAerolinea = vueloAero?.HorarioSalida,
                HorarioLlegadaAerolinea = vueloAero?.HorarioLlegada,
                Aeronave = vueloAero?.Aeronave,
                EstadoVueloAerolinea = vueloAero?.Estado,
                SincronizadoConAerolinea = vueloAero is not null
            };
        });

        return Ok(combinados);
    }
}