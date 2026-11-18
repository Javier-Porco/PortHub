using Microsoft.EntityFrameworkCore;
using PortHub.API.Context;
using PortHub.API.DTOs;
using PortHub.API.Interfaces;
using PortHub.API.Models;

namespace PortHub.API.Services;

public class AerolineaDbService(PortHubDbContext context) : IAerolineaService
{
    private readonly PortHubDbContext _context = context;

    public async Task<List<AerolineaResponseDto>> ObtenerTodasAsync()
    {
        var aerolineas = await _context.Aerolineas.AsNoTracking().ToListAsync();
        return aerolineas.Select(ADto).ToList();
    }

    public async Task<AerolineaResponseDto?> ObtenerPorIdAsync(int id)
    {
        var aerolinea = await _context.Aerolineas.FindAsync(id);
        return aerolinea == null ? null : ADto(aerolinea);
    }

    public async Task<bool> ExisteCodigoAsync(string codigo)
    {
        // El código se guarda en mayúsculas, así que se compara en mayúsculas ("arg" y "ARG" son la misma).
        return await _context.Aerolineas.AnyAsync(a => a.Codigo == codigo.Trim().ToUpper());
    }

    public async Task<AerolineaResponseDto> CrearAsync(CrearAerolineaDto dto)
    {
        var aerolinea = new Aerolinea
        {
            Nombre = dto.Nombre,
            Codigo = dto.Codigo.Trim().ToUpper(),
            Pais = dto.Pais,
            DireccionBase = dto.DireccionBase
        };
        _context.Aerolineas.Add(aerolinea);
        await _context.SaveChangesAsync();
        return ADto(aerolinea);
    }

    public async Task<bool> ActualizarAsync(int id, ActualizarAerolineaDto dto)
    {
        var aerolinea = await _context.Aerolineas.FindAsync(id);
        if (aerolinea == null) return false;

        aerolinea.Nombre = dto.Nombre;
        aerolinea.Pais = dto.Pais;
        aerolinea.DireccionBase = dto.DireccionBase;
        await _context.SaveChangesAsync();
        return true;
    }

    // Pasa de entidad (Models) a DTO de respuesta. Un solo lugar para el mapeo.
    private static AerolineaResponseDto ADto(Aerolinea a) => new()
    {
        Id = a.Id,
        Nombre = a.Nombre,
        Codigo = a.Codigo,
        Pais = a.Pais,
        DireccionBase = a.DireccionBase
    };
}