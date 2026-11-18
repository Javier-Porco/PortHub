using Microsoft.EntityFrameworkCore;
using PortHub.API.Context;
using PortHub.API.DTOs;
using PortHub.API.Interfaces;
using PortHub.API.Models;

namespace PortHub.API.Services;

public class EmbarqueDbService(PortHubDbContext context, IAeroLinkService aeroLink) : IEmbarqueService
{
    private readonly PortHubDbContext _context = context;
    private readonly IAeroLinkService _aeroLink = aeroLink;

    public async Task<ResultadoRegistroEmbarque> RegistrarAsync(RegistroEmbarqueDto dto)
    {
        var ticket = dto.NumeroTicket.Trim();

        // 1. ¿Este ticket ya pasó por el control? Se revisa primero porque no necesita llamar a AeroLink.
        if (await _context.Embarques.AnyAsync(e => e.NumeroTicket == ticket))
            return Fallo(EstadoRegistroEmbarque.TicketYaUtilizado, $"El ticket {ticket} ya fue registrado.");

        // 2. Validar el ticket con AeroLink. Si AeroLink no responde, sale una HttpRequestException
        //    que maneja el controller (503).
        var validacion = await _aeroLink.ValidarTicketAsync(ticket);
        if (validacion == null || !validacion.EsValido)
            return Fallo(EstadoRegistroEmbarque.TicketInvalido, $"AeroLink rechazó el ticket {ticket}.");

        // 3. El vuelo del ticket tiene que tener un slot activo en el aeropuerto...
        var slot = await _context.Slots.FirstOrDefaultAsync(s =>
            s.CodigoVuelo == validacion.CodigoVuelo && s.Estado != EstadoSlot.Liberado);
        if (slot == null)
            return Fallo(EstadoRegistroEmbarque.VueloSinSlot,
                $"El vuelo {validacion.CodigoVuelo} no tiene un slot activo en el aeropuerto.");

        // ...y el pasajero tiene que estar en el gate de ese slot.
        if (!string.Equals(slot.Gate, dto.Gate.Trim(), StringComparison.OrdinalIgnoreCase))
            return Fallo(EstadoRegistroEmbarque.GateIncorrecto,
                $"El vuelo {validacion.CodigoVuelo} embarca por el gate {slot.Gate}, no por {dto.Gate}.");

        // 4. Todo bien: se registra el acceso.
        var embarque = new Embarque
        {
            NumeroTicket = ticket,
            CodigoVuelo = validacion.CodigoVuelo,
            Gate = slot.Gate,
            HorarioAcceso = DateTime.UtcNow
        };
        _context.Embarques.Add(embarque);
        await _context.SaveChangesAsync();

        return new ResultadoRegistroEmbarque
        {
            Estado = EstadoRegistroEmbarque.Registrado,
            Embarque = ADto(embarque)
        };
    }

    public async Task<List<EmbarqueResponseDto>> ObtenerTrazabilidadAsync(string? gate)
    {
        var query = _context.Embarques.AsQueryable();
        if (!string.IsNullOrWhiteSpace(gate))
        {
            var gateBuscado = gate.Trim().ToUpper();
            query = query.Where(e => e.Gate == gateBuscado);
        }

        return await query
            .OrderByDescending(e => e.HorarioAcceso)
            .Select(e => new EmbarqueResponseDto
            {
                Id = e.Id,
                NumeroTicket = e.NumeroTicket,
                CodigoVuelo = e.CodigoVuelo,
                Gate = e.Gate,
                HorarioAcceso = e.HorarioAcceso
            })
            .ToListAsync();
    }

    private static ResultadoRegistroEmbarque Fallo(EstadoRegistroEmbarque estado, string detalle) =>
        new() { Estado = estado, Detalle = detalle };

    private static EmbarqueResponseDto ADto(Embarque e) => new()
    {
        Id = e.Id,
        NumeroTicket = e.NumeroTicket,
        CodigoVuelo = e.CodigoVuelo,
        Gate = e.Gate,
        HorarioAcceso = e.HorarioAcceso
    };
}