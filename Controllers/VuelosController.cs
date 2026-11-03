using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortHub.API.Data;
using PortHub.API.Domain.DTOs;
using PortHub.API.Domain.Enums;
using PortHub.API.Services;

namespace PortHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VuelosController : ControllerBase
{
    private readonly PortHubDbContext _context;

    public VuelosController(PortHubDbContext context)
    {
        _context = context;
    }

    // Desafío Adicional: Consulta unificada combinando datos de ambas APIs
    [HttpGet]
    public async Task<IActionResult> GetVuelosCombinados()
    {
        // 1. Obtenemos los slots locales registrados en PortHub
        var slotsLocales = await _context.Slots
            .Where(s => s.Estado != EstadoSlot.Liberado)
            .ToListAsync();

        // 2. Mapeamos la lista combinando los datos operativos del aeropuerto
        var resultado = slotsLocales.Select(s => new
        {
            codigoVuelo = s.CodigoVuelo,
            codigoAerolinea = s.CodigoAerolinea,
            horarioProgramado = s.HorarioProgramado,
            pistaAsignada = s.Pista,
            gateAsignado = s.Gate,
            estadoSlot = s.Estado.ToString().ToLower()
        });

        return Ok(resultado);
    }
}