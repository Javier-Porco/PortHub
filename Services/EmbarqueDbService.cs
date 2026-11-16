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

    public async Task<EmbarqueResponseDto?> RegistrarAsync(RegistroEmbarqueDto dto)
    {
        // 1. Validar el ticket con AeroLink
        var esValido = await _aeroLink.ValidarTicketAsync(dto.NumeroTicket);
        if (!esValido) return null;

        // 2. Registrar el acceso
        var embarque = new Embarque
        {
            NumeroTicket = dto.NumeroTicket,
            Gate = dto.Gate,
            HorarioAcceso = DateTime.UtcNow
        };
        _context.Embarques.Add(embarque);
        await _context.SaveChangesAsync();

        return new EmbarqueResponseDto
        {
            Id = embarque.Id,
            NumeroTicket = embarque.NumeroTicket,
            Gate = embarque.Gate,
            HorarioAcceso = embarque.HorarioAcceso
        };
    }

    public async Task<List<EmbarqueResponseDto>> ObtenerTrazabilidadAsync(string? gate)
    {
        var query = _context.Embarques.AsQueryable();
        if (!string.IsNullOrEmpty(gate))
            query = query.Where(e => e.Gate == gate);

        return await query
            .OrderByDescending(e => e.HorarioAcceso)
            .Select(e => new EmbarqueResponseDto
            {
                Id = e.Id,
                NumeroTicket = e.NumeroTicket,
                Gate = e.Gate,
                HorarioAcceso = e.HorarioAcceso
            })
            .ToListAsync();
    }
}